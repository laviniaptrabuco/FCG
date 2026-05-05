# FIAP Cloud Games (FCG) — Fase 1

API REST em **.NET 8** para gerenciamento de usuários e biblioteca de jogos da plataforma **FIAP Cloud Games**.

---

## Sumário

- [Objetivo](#objetivo)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Funcionalidades](#funcionalidades)
- [Pré-requisitos](#pré-requisitos)
- [Como Executar](#como-executar)
- [Criando o primeiro Admin](#criando-o-primeiro-admin)
- [Testes](#testes)
- [Validações de Senha](#validações-de-senha)
- [Problemas Comuns](#problemas-comuns)
- [DDD](#ddd)
- [Estrutura do Banco de Dados](#estrutura-do-banco-de-dados)
- [Grupo](#grupo)

---

## Objetivo

Este projeto é o **Tech Challenge da Fase 1** da pós-graduação em Arquitetura de Sistemas .NET (NETT) da FIAP PosTech.

O objetivo é construir o MVP da **FIAP Cloud Games** — uma plataforma de venda de jogos digitais voltada à educação em tecnologia. Nesta fase, o foco é o serviço de **cadastro de usuários e biblioteca de jogos adquiridos**, que servirá de base para as próximas fases do projeto.

Requisitos atendidos nesta fase:

- Cadastro de usuários com validação de e-mail e senha forte
- Autenticação via token JWT com dois níveis de acesso (Usuário e Administrador)
- Gerenciamento de jogos: cadastro, promoções e filtro por gênero
- Biblioteca pessoal de jogos por usuário
- Middleware de tratamento de erros e logs estruturados
- Documentação via Swagger/OpenAPI
- 80 testes unitários cobrindo as regras de negócio
- TDD aplicado no módulo de validação de senha
- Persistência com SQLite (local) e MongoDB (Docker)
- Arquitetura em camadas seguindo princípios de DDD

---

## Arquitetura

O projeto segue uma arquitetura em **camadas** inspirada no **DDD (Domain-Driven Design)**:

```
FCG/
├── src/
│   ├── FCG.Domain          # Entidades, interfaces, value objects, exceções de domínio
│   ├── FCG.Application     # Serviços de aplicação, DTOs, interfaces de infraestrutura
│   ├── FCG.Infrastructure  # EF Core/MongoDB, repositórios, JWT, BCrypt
│   └── FCG.API             # Controllers, Middleware, configuração, Swagger
└── tests/
    └── FCG.UnitTests       # Testes unitários (xUnit + Moq + FluentAssertions)
```

**Fluxo de dependências:**
```
API → Application → Domain
Infrastructure → Domain
Infrastructure → Application
```

---

## Tecnologias

| Tecnologia | Uso |
|---|---|
| .NET 8 | Framework principal |
| ASP.NET Core MVC | Controllers REST |
| Entity Framework Core 8 | ORM para SQLite (execução local) |
| SQLite | Banco de dados para execução local (sem instalação) |
| MongoDB | Banco de dados para execução via Docker |
| JWT Bearer | Autenticação |
| BCrypt.Net | Hash de senhas |
| Swagger / OpenAPI | Documentação interativa |
| xUnit | Framework de testes |
| Moq | Mocking em testes |
| FluentAssertions | Asserções legíveis em testes |
| Docker / Docker Compose | Containerização |

---

## Funcionalidades

### Autenticação (público)
- `POST /api/auth/register` — Cadastro de usuário
- `POST /api/auth/login` — Login com retorno de token JWT

### Usuários (autenticado)
- `GET /api/users` — Listar todos os usuários *(Admin)*
- `GET /api/users/{id}` — Obter usuário com biblioteca de jogos
- `PUT /api/users/{id}` — Atualizar dados do usuário
- `PATCH /api/users/{id}/password` — Alterar senha
- `DELETE /api/users/{id}` — Desativar usuário *(Admin)*
- `PATCH /api/users/{id}/activate` — Reativar usuário *(Admin)*
- `POST /api/users/admin` — Criar administrador *(Admin)*
- `POST /api/users/{id}/games/{gameId}` — Adicionar jogo à biblioteca
- `DELETE /api/users/{id}/games/{gameId}` — Remover jogo da biblioteca

### Jogos
- `GET /api/games` — Listar todos os jogos *(público)*
- `GET /api/games/{id}` — Obter jogo por ID *(público)*
- `GET /api/games/genre/{genre}` — Filtrar por gênero *(público)*
- `POST /api/games` — Cadastrar jogo *(Admin)*
- `PUT /api/games/{id}` — Atualizar jogo *(Admin)*
- `PATCH /api/games/{id}/promotion` — Aplicar preço promocional *(Admin)*
- `DELETE /api/games/{id}/promotion` — Remover promoção *(Admin)*
- `DELETE /api/games/{id}` — Desativar jogo *(Admin)*

---

## Pré-requisitos

Escolha uma das duas formas de executar o projeto:

### Opção A — Local (sem Docker)

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

Verifique a instalação:
```bash
dotnet --version
# deve exibir 8.x.x
```

Não é necessário instalar banco de dados — o SQLite é criado automaticamente.

### Opção B — Docker

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

Verifique a instalação:
```bash
docker --version
docker compose version
```

---

## Como Executar

### Clonando o repositório

```bash
git clone https://github.com/laviniaptrabuco/FCG.git
cd FCG
```

### Opção A — Local (SQLite)

```bash
cd src/FCG.API
dotnet run
```

- Swagger: `http://localhost:5000/swagger`
- O arquivo `fcg_dev.db` é criado automaticamente na pasta `src/FCG.API`

### Opção B — Docker Compose (MongoDB)

```bash
docker-compose up --build
```

- Swagger: `http://localhost:5000/swagger`
- O banco MongoDB sobe automaticamente junto com a API

### Resumo por ambiente

| Comando | Banco | URL |
|---|---|---|
| `dotnet run` | SQLite (arquivo local) | http://localhost:5000/swagger |
| `docker-compose up --build` | MongoDB (container) | http://localhost:5000/swagger |

---

## Criando o primeiro Admin

Registre um usuário normalmente via Swagger (`POST /api/auth/register`):

```json
{
  "name": "Admin FIAP",
  "email": "admin@fiap.com",
  "password": "Admin@123!"
}
```

Depois promova o usuário para Admin diretamente no banco:

### SQLite — opção 1: linha de comando

Verifique se o `sqlite3` está disponível:
```bash
sqlite3 --version
```

Se estiver, rode dentro da pasta `src/FCG.API`:
```bash
sqlite3 fcg_dev.db "UPDATE Users SET Role = 'Admin' WHERE Email = 'admin@fiap.com';"
```

No Windows, se o `sqlite3` não estiver instalado, baixe o `sqlite3.exe` em https://sqlite.org/download.html (pacote *sqlite-tools-win*), coloque na pasta `src/FCG.API` e execute:
```powershell
.\sqlite3.exe fcg_dev.db "UPDATE Users SET Role = 'Admin' WHERE Email = 'admin@fiap.com';"
```

### SQLite — opção 2: DB Browser for SQLite (interface gráfica)

1. Baixe em https://sqlitebrowser.org
2. Abra o arquivo `src/FCG.API/fcg_dev.db`
3. Vá em **Browse Data → tabela Users**
4. Edite o campo `Role` do usuário de `User` para `Admin`
5. Clique em **Write Changes**

### MongoDB (Docker)

Acesse o container e execute:
```bash
docker exec -it fcg-mongo mongosh
use fcg
db.users.updateOne({ email: "admin@fiap.com" }, { $set: { Role: "Admin" } })
```

> Após promover, faça login novamente para obter um novo token JWT com a role `Admin`.

A partir daí, use `POST /api/users/admin` para criar novos admins via API sem precisar acessar o banco.

---

## Testes

Execute a partir da raiz do projeto:

```bash
# Executar todos os testes
dotnet test

# Listar os testes disponíveis
dotnet test --list-tests

# Com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

Cobertura dos testes unitários (80 casos):

| Arquivo | O que testa |
|---|---|
| `UserTests` | Entidade User: criação, validações, métodos de domínio |
| `GameTests` | Entidade Game: criação, promoções, validações |
| `PasswordValidatorTests` | Regras de senha (desenvolvido com TDD) |
| `AuthServiceTests` | Login e registro via serviço de aplicação |
| `GameServiceTests` | CRUD de jogos via serviço de aplicação |
| `UserServiceTests` | Gerenciamento de usuários e biblioteca |

---

## Validações de Senha

A senha deve ter **no mínimo 8 caracteres** contendo:
- Pelo menos **1 letra**
- Pelo menos **1 número**
- Pelo menos **1 caractere especial**: `@` `$` `!` `%` `*` `?` `&` `_` `-` `#` `^`

Exemplo válido: `Admin@123!`

---

## Problemas Comuns

### Porta 5000 já está em uso

```
Failed to bind to address http://localhost:5000: address already in use
```

Identifique e encerre o processo que está usando a porta:

**Windows:**
```powershell
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

**Linux/macOS:**
```bash
lsof -i :5000
kill -9 <PID>
```

Ou altere a porta no arquivo `src/FCG.API/Properties/launchSettings.json`.

---

### Erro ao rodar `dotnet run`: SDK não encontrado

```
No .NET SDKs were found
```

Instale o .NET 8 SDK em https://dotnet.microsoft.com/download/dotnet/8.0 e reinicie o terminal.

---

### Arquivo `fcg_dev.db` com permissão negada (Windows)

```
SQLite Error 14: 'unable to open database file'
```

Verifique se outro processo (como o DB Browser) está com o arquivo aberto. Feche-o e tente novamente.

---

### Docker: porta 5000 ou 27017 em uso

```
Bind for 0.0.0.0:5000 failed: port is already allocated
```

Pare containers existentes e suba novamente:
```bash
docker-compose down
docker-compose up --build
```

---

### Docker: API sobe mas não conecta ao MongoDB

A API pode iniciar antes do MongoDB estar pronto. Aguarde alguns segundos e acesse o Swagger novamente. O `docker-compose.yml` já configura `depends_on`, mas o MongoDB pode levar alguns segundos para aceitar conexões.

---

### Token JWT retorna 401 após promover para Admin

O token JWT carrega a role no momento do login. Após alterar a role no banco, faça logout e login novamente para gerar um novo token com a role `Admin`.

---

### `dotnet test` falha com erro de build

```bash
dotnet restore
dotnet build
dotnet test
```

---

## DDD

A documentação de Event Storming está no miro: https://miro.com/app/board/uXjVHc1_4PQ=/?share_link_id=124714599020

### Contextos modelados

| Contexto | Fluxos cobertos |
|---|---|
| Registro de Usuário | Cadastro, validações, unicidade de e-mail |
| Autenticação | Login, geração de JWT, usuário inativo |
| Gestão de Jogos | Cadastro, promoções, desativação |
| Biblioteca do Usuário | Adicionar/remover jogos, regras de duplicata |

---

## Estrutura do Banco de Dados

```
Users
├── Id (PK, GUID)
├── Name
├── Email (UNIQUE)
├── PasswordHash
├── Role (User | Admin)
├── CreatedAt
└── IsActive

Games
├── Id (PK, GUID)
├── Title (UNIQUE)
├── Description
├── Genre
├── Price
├── PromotionalPrice (opcional)
├── CreatedAt
└── IsActive

UserGames
├── UserId (FK → Users)
├── GameId (FK → Games)
└── AcquiredAt
```

---

## Grupo

| Nome | Discord |
|---|---|
| Lavínia Pedrosa Trabuco | trabuqueira |

---

*Fase 1 — FIAP PosTech · Arquitetura de Sistemas .NET · NETT · 2026*
