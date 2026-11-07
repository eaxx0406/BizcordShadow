# 0008: Lokal infrastruktur med docker compose og user-secrets

Status: Accepteret (2026-11-06)

## Kontekst
Alle udviklere skal kunne køre systemet lokalt med samme database og schema. Connection strings indeholder passwords, som ikke må ende i git.

Alternativer for databasen:
- **Hver installerer PostgreSQL selv:** forskellige versioner og opsætninger.
- **`docker run` manuelt:** ens, men kommandoen skal huskes og deles.
- **`docker compose`:** opsætningen ligger som fil i repoet.

Alternativer for secrets:
- **I `appsettings.json`:** nemt, men passwords havner i git.
- **Miljøvariabler:** fungerer, men skal sættes på hver maskine/terminal.
- **.NET user-secrets:** gemmes per bruger uden for repoet og læses automatisk i Development.

## Beslutning
- Databasen kører via **`compose.yaml`** i roden. Schema og seed ligger i `database/chatservice/` og køres automatisk første gang databasen oprettes (`/docker-entrypoint-initdb.d`).
- Services (ChatService, GatewayAPI, frontend) køres stadig fra Visual Studio for nem debugging. Vi laver Dockerfiles, når hele systemet skal kunne køre i containere.
- Connection strings gemmes med **user-secrets** (`ConnectionStrings:ChatDb`).

## Konsekvenser
- ✅ `docker compose up -d` giver alle samme database med samme schema og testdata.
- ✅ Passwords er ikke i git.
- ⚠️ Init-scripts kører kun på en tom database. Ændringer i schema kræver `docker compose down -v` (sletter alle lokale data).
- ⚠️ Hver udvikler skal selv sætte user-secret én gang (kommandoen står i beskrivelsen af PR #1).
- ⚠️ Lokale standard-passwords i `compose.yaml` er kun til udvikling og må ikke bruges i produktion.
