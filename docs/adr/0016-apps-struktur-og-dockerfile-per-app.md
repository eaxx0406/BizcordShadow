# 0016: apps/-struktur med Dockerfile per app

Status: Accepteret (2026-11-07)

## Kontekst
Kurset kræver denne struktur for hver microservice:

```
apps/
  [microservice-domain]-microservice/
    src/
    tests/
    Dockerfile
    README.md
```

Indtil nu lå projekterne direkte i roden (`ChatService/`, `ChatService.Contracts/`, `GatewayAPI/`, `Bizcord/`) og databasescripts i `database/chatservice/`. Strukturen nævner kun microservices, så vi skulle også beslutte, hvor gatewayen og frontenden skal ligge.

Alternativer for gateway og frontend:
- **A: Lad dem ligge i roden** og flyt kun ChatService. Færre ændringer, men så har repoet to forskellige strukturer.
- **B: Giv dem samme form** under `apps/`. Alt, der kan deployes, ligger og ser ens ud.

## Beslutning
- Vi vælger **B**. Hver app, der kan deployes, har sin egen mappe under `apps/` med `src/`, `Dockerfile` og `README.md` (og `tests/`, når der er tests):
  - `apps/chat-microservice/`: `src/ChatService`, `src/ChatService.Contracts`, `tests/ChatService.Tests`, plus `database/` med schema og seed. Databasen hører til servicen (ADR 0002), så scripts ligger sammen med den.
  - `apps/api-gateway/`: `src/GatewayAPI`.
  - `apps/web/`: `src/Bizcord` (frontend).
- Fælles ting bliver i roden: solution-filen, `compose.yaml`, `docs/adr/` og README.
- **Én Dockerfile per app**, multi-stage: byg med `sdk:10.0`, kør med `aspnet:10.0` (lille image uden SDK og kildekode). Containeren lytter på port 8080 og kører som en bruger uden admin-rettigheder.
- **Dockerfiler bygges fra roden af repoet** (`docker build -f apps/<app>/Dockerfile .`), fordi `web` bruger `ChatService.Contracts` fra `chat-microservice` (ADR 0007). En `.dockerignore` i roden holder `bin/`, `obj/`, `.git/` osv. ude.
- Konfiguration, der i dag ligger i user-secrets (connection string) eller peger på `localhost`, gives som miljøvariabler i containeren (`ConnectionStrings__ChatDb`, `GatewayApi__BaseUrl`).

## Konsekvenser
- ✅ Følger kursets struktur, og hver app er selvstændig: kode, tests, image og dokumentation ét sted.
- ✅ Nye microservices får samme form (`apps/<domæne>-microservice/`).
- ✅ Images kan bygges og køres uafhængigt af Visual Studio.
- ⚠️ Stierne er længere, og eksisterende kloner skal hente de nye stier. Gamle mapper med `bin/` og `obj/` skal slettes manuelt.
- ⚠️ `web` refererer på tværs af apps (`../../../chat-microservice/src/ChatService.Contracts`). Det er bevidst (Contracts er servicens offentlige kontrakt), men det betyder, at web-image skal bygges fra roden. Bliver det et problem, kan Contracts udgives som en NuGet-pakke.
- ⚠️ At køre alt i `docker compose` er ikke klar endnu: Ocelot-routes peger på `localhost:5210`. Det løses med servicenavne og miljøspecifik konfiguration, når vi tager det skridt.
