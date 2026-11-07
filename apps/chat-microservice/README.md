# chat-microservice

Ejer chats, deltagelse (hvem er med i hvilken chat) og beskeder. Har sin egen PostgreSQL-database (ADR 0002).

| Mappe | Indhold |
|---|---|
| `src/ChatService` | Web API. Controllere → MediatR-handlere (commands/queries, ADR 0004) → repositories med Dapper (ADR 0005). |
| `src/ChatService.Contracts` | De DTO'er, andre må kende (requests/responses, ADR 0007). Bruges også af `web`. |
| `tests/ChatService.Tests` | xUnit-tests. |
| `database` | `01-schema.sql` og `02-seed.sql`. Køres af Postgres-containeren første gang (se `compose.yaml` i roden). |

## Endpoints

Alle kaldes udefra gennem `api-gateway`. `userId` sendes af klienten, indtil der findes login (ADR 0013).

| Metode og sti | Beskrivelse |
|---|---|
| `GET /api/chats?userId=` | Mine chats med seneste besked |
| `POST /api/chats` | Opret chat med medlemmer (ADR 0012) |
| `GET /api/chats/{id}?userId=` | Én chat med medlemmer (kun for medlemmer) |
| `POST /api/chats/{id}/participants` | Tilføj deltager (ADR 0010) |
| `GET /api/chats/{id}/messages?userId=&before=&limit=` | Beskeder, keyset paging (ADR 0014) |
| `POST /api/chats/{id}/messages` | Send besked |

Fejl returneres som ProblemDetails: 400 / 403 / 404 / 409 / 500 (ADR 0011, 0013). Eksempler i `src/ChatService/ChatService.http`.

## Kør

- Lokalt: start `ChatService` med **http**-profilen (port 5210). Kræver connection string i user-secrets, se README i roden.
- Tests: `dotnet test` (fra roden eller fra `tests/ChatService.Tests`).
- Docker image (fra roden af repoet): `docker build -f apps/chat-microservice/Dockerfile -t bizcord/chat-microservice .`
  Connection string gives som miljøvariabel: `ConnectionStrings__ChatDb`.
