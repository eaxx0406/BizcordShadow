# Bizcord

Chat-platform bygget som microservices i .NET 10.

```
Bizcord (frontend)  ──▶  GatewayAPI (Ocelot)  ──▶  ChatService  ──▶  PostgreSQL (chatdb)
   Razor Pages              :5065                     :5210             :5432 (Docker)
```

## Projekter

| Projekt | Rolle |
|---|---|
| `Bizcord` | Frontend (Razor Pages). Kalder kun gatewayen. |
| `GatewayAPI` | API Gateway med Ocelot. Routes i `GatewayAPI/Ocelot.json`. |
| `ChatService` | Microservice for chats. MediatR (commands/handlers) + Dapper. |
| `ChatService.Contracts` | ChatService' offentlige DTO'er (requests/responses). |

Arkitekturbeslutninger er beskrevet i [`docs/adr/`](docs/adr/README.md).

## Kom i gang

**Krav:** .NET 10 SDK, Docker Desktop, Visual Studio (eller `dotnet` CLI).

1. **Start databasen**
   ```
   docker compose up -d
   ```
   Schema og testdata i `database/chatservice/` oprettes automatisk første gang.

2. **Sæt connection string** (én gang per maskine – gemmes uden for repoet)
   ```
   dotnet user-secrets set "ConnectionStrings:ChatDb" "Host=localhost;Port=5432;Database=chatdb;Username=bizcord;Password=bizcord" --project ChatService
   ```

3. **Start services** – i Visual Studio: højreklik på solution → *Configure Startup Projects* → *Multiple*, og vælg `Bizcord`, `GatewayAPI` og `ChatService` (brug **http**-profilen).

4. **Test** – klik *Create chat* i frontend, eller kør `GatewayAPI/GatewayAPI.http`.

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

## Konventioner

- Database first, Postgres-navngivning med `snake_case` ([ADR 0005](docs/adr/0005-dapper-og-database-first.md), [ADR 0006](docs/adr/0006-postgres-navngivning-snake-case.md)).
- Hver service ejer sin egen database og sine egne interfaces – ingen delt domænekode ([ADR 0002](docs/adr/0002-database-per-service.md), [ADR 0009](docs/adr/0009-ingen-delt-domaenekode-mellem-services.md)).
- Nyt endpoint, der skal kunne kaldes udefra = ny route i `GatewayAPI/Ocelot.json`.
- Arbejd på en feature-branch og merge til `master` via pull request.
