# Bizcord

Chat-platform bygget som microservices i .NET 10.

```
web (frontend)  ──▶  api-gateway (Ocelot)  ──▶  chat-microservice  ──▶  PostgreSQL (chatdb)
 Razor Pages            :5065                       :5210                 :5432 (Docker)
```

## Struktur

Alt, der kan deployes, ligger under `apps/` med samme form ([ADR 0016](docs/adr/0016-apps-struktur-og-dockerfile-per-app.md)):

```
apps/
  chat-microservice/
    src/        ChatService (API) og ChatService.Contracts (DTO'er)
    tests/      ChatService.Tests (xUnit)
    database/   schema og testdata (kun denne service' database)
    Dockerfile
    README.md
  api-gateway/
    src/        GatewayAPI (Ocelot)
    Dockerfile
    README.md
  web/
    src/        Bizcord (Razor Pages)
    Dockerfile
    README.md
docs/adr/       arkitekturbeslutninger
compose.yaml    lokal infrastruktur (database)
```

| App | Rolle |
|---|---|
| [`chat-microservice`](apps/chat-microservice/README.md) | Chats, deltagere og beskeder. MediatR (commands/queries) + Dapper. |
| [`api-gateway`](apps/api-gateway/README.md) | API Gateway med Ocelot. Det eneste, frontenden kalder. |
| [`web`](apps/web/README.md) | Frontend (Razor Pages). Kalder kun gatewayen. |

Arkitekturbeslutninger er beskrevet i [`docs/adr/`](docs/adr/README.md).

## Kom i gang

**Krav:** .NET 10 SDK, Docker Desktop, Visual Studio (eller `dotnet` CLI).

1. **Start databasen**
   ```
   docker compose up -d
   ```
   Schema og testdata i `apps/chat-microservice/database/` oprettes automatisk første gang.

2. **Sæt connection string** (én gang per maskine – gemmes uden for repoet)
   ```
   dotnet user-secrets set "ConnectionStrings:ChatDb" "Host=localhost;Port=5432;Database=chatdb;Username=bizcord;Password=bizcord" --project apps/chat-microservice/src/ChatService
   ```

3. **Start services** – i Visual Studio: højreklik på solution → *Configure Startup Projects* → *Multiple*, og vælg `Bizcord`, `GatewayAPI` og `ChatService` (brug **http**-profilen).

4. **Test** – opret en chat i frontend, eller kør `apps/api-gateway/src/GatewayAPI/GatewayAPI.http`.

5. **Kør tests**
   ```
   dotnet test
   ```

## Database

| | |
|---|---|
| Host / port | `localhost` / `5432` |
| Database | `chatdb` |
| Bruger / password | `bizcord` / `bizcord` (kun til lokal udvikling) |

- Se data: `docker compose exec chatdb psql -U bizcord -d chatdb -c "select * from chat"`
- **Nulstil** (efter ændringer i schema/seed – sletter alle data):
  ```
  docker compose down -v
  docker compose up -d
  ```

## Docker images

Hver app har sin egen `Dockerfile`. Byg fra **roden af repoet**, fordi `web` bruger `ChatService.Contracts` fra `chat-microservice`:

```
docker build -f apps/chat-microservice/Dockerfile -t bizcord/chat-microservice .
docker build -f apps/api-gateway/Dockerfile       -t bizcord/api-gateway .
docker build -f apps/web/Dockerfile               -t bizcord/web .
```

At køre det hele i `docker compose` er et senere skridt: gatewayen og frontenden skal så finde services via navne i Docker i stedet for `localhost`.

## Konventioner

- Database first, Postgres-navngivning med `snake_case` ([ADR 0005](docs/adr/0005-dapper-og-database-first.md), [ADR 0006](docs/adr/0006-postgres-navngivning-snake-case.md)).
- Hver service ejer sin egen database og sine egne interfaces – ingen delt domænekode ([ADR 0002](docs/adr/0002-database-per-service.md), [ADR 0009](docs/adr/0009-ingen-delt-domaenekode-mellem-services.md)).
- Nyt endpoint, der skal kunne kaldes udefra = ny route i `apps/api-gateway/src/GatewayAPI/Ocelot.json`.
- Arbejd på en feature-branch og merge til `master` via pull request.
