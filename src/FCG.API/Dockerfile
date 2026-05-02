FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/FCG.API/FCG.API.csproj", "src/FCG.API/"]
COPY ["src/FCG.Application/FCG.Application.csproj", "src/FCG.Application/"]
COPY ["src/FCG.Domain/FCG.Domain.csproj", "src/FCG.Domain/"]
COPY ["src/FCG.Infrastructure/FCG.Infrastructure.csproj", "src/FCG.Infrastructure/"]

RUN dotnet restore "src/FCG.API/FCG.API.csproj"

COPY . .
WORKDIR "/src/src/FCG.API"
RUN dotnet build "FCG.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FCG.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG.API.dll"]
