# api-gateway

Det eneste indgangspunkt udefra (ADR 0001). Bygget med Ocelot: hver request matches mod en route i `src/GatewayAPI/Ocelot.json` og sendes videre til den rigtige service.

- **Én route per endpoint** fungerer som hvidliste: et endpoint, der ikke står i `Ocelot.json`, kan ikke kaldes udefra (svarer 404).
- Statuskoder og ProblemDetails fra services sendes uændret videre.

## Kør

- Lokalt: start `GatewayAPI` med **http**-profilen (port 5065). Routes peger på `localhost:5210` (chat-microservice).
- Eksempler: `src/GatewayAPI/GatewayAPI.http`.
- Docker image (fra roden af repoet): `docker build -f apps/api-gateway/Dockerfile -t bizcord/api-gateway .`
  I Docker skal routes pege på servicenavne i stedet for `localhost`. Det kommer, når vi kører det hele i compose.
