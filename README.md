# 🎮 FIAP Cloud Games (FCG) — Fase 1

API REST em **.NET 8** para gerenciamento de usuários e biblioteca de jogos da plataforma **FIAP Cloud Games**.

---

## 📋 Sumário

- [Sobre o Projeto](#sobre-o-projeto)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Funcionalidades](#funcionalidades)
- [Como Executar](#como-executar)
- [Endpoints](#endpoints)
- [Testes](#testes)
- [DDD](#ddd)

---

## Sobre o Projeto

A FCG é uma plataforma de jogos digitais voltada à educação em tecnologia. Esta **Fase 1** entrega o MVP com:

- Cadastro e autenticação de usuários via **JWT**
- Dois níveis de acesso: **Usuário** e **Administrador**
- Gerenciamento de jogos e biblioteca pessoal
- Promoções em jogos
- Testes unitários com cobertura das regras de negócio

---

## Arquitetura

O projeto segue uma arquitetura em **camadas** inspirada no **DDD (Domain-Driven Design)**:

```
FCG/
├── src/
│   ├── FCG.Domain          # Entidades, interfaces, value objects, exceções
│   ├── FCG.Application     # Serviços de aplicação, DTOs, interfaces de infra
│   ├── FCG.Infrastructure  # EF Core, repositórios, JWT, BCrypt
│   └── FCG.API             # Controllers, Middleware, configuração
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
| Entity Framework Core 8 | ORM / Migrations |
| SQL Server | Banco de dados |
| JWT Bearer | Autenticação |
| BCrypt.Net | Hash de senhas |
| Swagger / OpenAPI | Documentação |
| xUnit | Framework de testes |
| Moq | Mocking |
| FluentAssertions | Asserções legíveis |
| Docker / Docker Compose | Containerização |

---

## Funcionalidades

### Autenticação
- `POST /api/auth/register` — Cadastro de usuário
- `POST /api/auth/login` — Login com retorno de token JWT

### Usuários (autenticado)
- `GET /api/users` — Listar usuários *(Admin)*
- `GET /api/users/{id}` — Obter usuário por ID
- `PUT /api/users/{id}` — Atualizar usuário
- `PATCH /api/users/{id}/password` — Alterar senha
- `DELETE /api/users/{id}` — Desativar usuário *(Admin)*
- `PATCH /api/users/{id}/activate` — Ativar usuário *(Admin)*
- `POST /api/users/admin` — Criar administrador *(Admin)*
- `POST /api/users/{id}/games/{gameId}` — Adicionar jogo à biblioteca
- `DELETE /api/users/{id}/games/{gameId}` — Remover jogo da biblioteca

### Jogos
- `GET /api/games` — Listar todos os jogos *(público)*
- `GET /api/games/{id}` — Obter jogo por ID *(público)*
- `GET /api/games/genre/{genre}` — Filtrar por gênero *(público)*
- `POST /api/games` — Cadastrar jogo *(Admin)*
- `PUT /api/games/{id}` — Atualizar jogo *(Admin)*
- `PATCH /api/games/{id}/promotion` — Aplicar promoção *(Admin)*
- `DELETE /api/games/{id}/promotion` — Remover promoção *(Admin)*
- `DELETE /api/games/{id}` — Desativar jogo *(Admin)*

---

## Como Executar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/pt-br/sql-server) ou [Docker](https://www.docker.com/)

### Opção 1 — Docker Compose (recomendado)

```bash
# Na raiz do projeto
docker-compose up --build
```

A API estará disponível em: `http://localhost:5000`  
Swagger: `http://localhost:5000/swagger`

### Opção 2 — Local com SQL Server

1. **Ajuste a connection string** em `src/FCG.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FiapCloudGames_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

2. **Restaure e execute:**

```bash
cd src/FCG.API
dotnet restore
dotnet run
```

As migrations serão aplicadas automaticamente na inicialização.

3. **Acesse o Swagger:** `https://localhost:5001/swagger`

### Criando o primeiro Admin

Após subir a API, registre um usuário normalmente:

```http
POST /api/auth/register
{
  "name": "Admin FIAP",
  "email": "admin@fiap.com",
  "password": "Admin@123!"
}
```

> Por segurança, promova o usuário para Admin diretamente no banco na primeira vez:
> ```sql
> UPDATE Users SET Role = 'Admin' WHERE Email = 'admin@fiap.com';
> ```
> Depois use o endpoint `POST /api/users/admin` para criar novos admins via API.

---

## Testes

```bash
# Executar todos os testes
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Listar testes
dotnet test --list-tests
```

Cobertura dos testes unitários:
- `UserTests` — entidade User (criação, validações, jogos)
- `GameTests` — entidade Game (criação, promoções, validações)
- `PasswordValidatorTests` — regras de senha (TDD)
- `AuthServiceTests` — login, registro (mocks)
- `GameServiceTests` — CRUD de jogos (mocks)
- `UserServiceTests` — gerenciamento de usuários e biblioteca (mocks)

---

## Validações de Senha

A senha deve ter **no mínimo 8 caracteres** contendo:
- Pelo menos **1 letra**
- Pelo menos **1 número**
- Pelo menos **1 caractere especial** (`@`, `$`, `!`, `%`, `*`, `?`, `&`, `_`, `-`, `#`, `^`)

Exemplo válido: `Senha@123`

---

## DDD

A documentação de DDD (Event Storming) está disponível no Miro:

> 📌 **[Link do Miro — Event Storming FCG](https://miro.com/app/board/SEU_BOARD_ID/)**

### Contextos modelados:
- **Criação de Usuários** — Registro, validação, autenticação
- **Criação de Jogos** — Cadastro, promoção, biblioteca

---

## Estrutura do Banco de Dados

```
Users
├── Id (PK)
├── Name
├── Email (UNIQUE)
├── PasswordHash
├── Role (User | Admin)
├── CreatedAt
└── IsActive

Games
├── Id (PK)
├── Title (UNIQUE)
├── Description
├── Genre
├── Price
├── PromotionalPrice
├── CreatedAt
└── IsActive

UserGames
├── UserId (FK → Users)
├── GameId (FK → Games)
└── AcquiredAt
```

---

## 👥 Grupo

| Nome | Discord |
|---|---|
| Seu Nome | @seuusername |

---

*Fase 1 — FIAP PosTech · NETT · 2024*
