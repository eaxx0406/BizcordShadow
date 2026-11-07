# web

Frontend i Razor Pages. Kalder **kun** `api-gateway` (ADR 0001), via `GatewayClient`.

- Bruger `ChatService.Contracts` fra `chat-microservice` til requests og svar (ADR 0007).
- "Dev login": UserService er uden for scope, så man vælger en testbruger i topbaren (gemmes i en cookie).
- Fejl fra services (ProblemDetails) vises som læsbare beskeder.

## Kør

- Lokalt: start `Bizcord` med **http**-profilen (port 5186). Gatewayens adresse står i `appsettings.json` (`GatewayApi:BaseUrl`).
- Docker image (fra roden af repoet, fordi Contracts ligger i `chat-microservice`):
  `docker build -f apps/web/Dockerfile -t bizcord/web .`
  Gatewayens adresse gives som miljøvariabel: `GatewayApi__BaseUrl`.
