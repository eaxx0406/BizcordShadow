# 0015: REST (JSON over HTTP) frem for GraphQL og gRPC

Status: Accepteret (2026-11-07)

## Kontekst
Frontend kalder ChatService gennem gatewayen (ADR 0001). Vi skal vælge, hvordan det API ser ud. Der er tre udbredte måder at lave et API mellem klient og service på:

- **REST:** én URL per ressource (`/api/chats/{id}/messages`), HTTP-metoder (GET/POST) og HTTP-statuskoder. Data som JSON.
- **GraphQL:** ét endpoint (typisk `POST /graphql`). Klienten skriver en forespørgsel og vælger selv, hvilke felter den vil have, også på tværs af ressourcer (chat + medlemmer + beskeder i ét kald). Et skema beskriver alle typer.
- **gRPC:** funktionskald over HTTP/2 med binære beskeder (Protocol Buffers). Kontrakten skrives i en `.proto`-fil, og klient- og serverkode genereres ud fra den. Den understøtter streaming begge veje.

Vores situation:
- Vi har **én klient**, som vi selv bygger. Vi kan give den præcis de data, hver skærm skal bruge.
- Gatewayen er **Ocelot**, som router på URL og HTTP-metode og fungerer som hvidliste over endpoints (ADR 0001).
- Fejl skal være tydelige og ens (ADR 0011: statuskoder plus ProblemDetails).
- Caching er en del af pensum, og HTTP har caching indbygget for GET.

## Beslutning
Vi bruger **REST med JSON**.

- **Hver skærm får sin egen DTO** i stedet for, at klienten vælger felter. Listen får `ChatSummaryDto`, chatten `ChatDetailsDto`, og beskederne `MessagePageDto`. Det løser det problem, GraphQL ellers løser (for meget eller for lidt data), og det passer med CQRS-light (ADR 0004): læsesiden laver præcis den form, klienten skal bruge.
- **Paging sker med query-parametre** (`?before=...&limit=...`, ADR 0014). Det er den samme idé som GraphQL's cursor-baserede "connections", bare uden GraphQL.
- **Statuskoder bærer fejlen** (400/403/404/409/500, ADR 0011 og 0013).
- **Realtid er ikke request/response.** Den løses separat (SignalR), og kommunikation mellem services sker asynkront med beskeder (RabbitMQ). REST bruges kun til klient → gateway → service.

Fravalgt:
- **GraphQL:** Den største fordel, at mange forskellige klienter kan hente præcis det, de vil, har vi ikke brug for med én klient. Til gengæld ville vi få:
  - Ét endpoint, så Ocelot ikke længere kan route eller hvidliste per endpoint.
  - Svært at cache, fordi forespørgsler typisk er POST til samme URL.
  - Fejl, der som regel kommer som `200 OK` med en `errors`-liste i stedet for statuskoder.
  - Risiko for N+1-forespørgsler i databasen, som kræver ekstra værktøj (DataLoader).
  - Et ekstra bibliotek og en ny måde at tænke på.
- **gRPC:** Den er hurtig og stærkt typet, og den er bedst mellem services. Men:
  - Browsere kan ikke kalde gRPC direkte (det kræver gRPC-Web og en proxy).
  - Beskederne er binære og kan ikke læses eller testes med en `.http`-fil.
  - Ocelot er bygget til HTTP/JSON.
  - Mellem vores services vil vi kommunikere asynkront med beskeder, ikke med synkrone kald.

## Konsekvenser
- ✅ Simpelt og velkendt. Kan testes med `.http`-filer, curl og browseren.
- ✅ Passer til gatewayen: én Ocelot-route per endpoint (ADR 0001).
- ✅ GET-svar kan caches med almindelig HTTP-caching, hvis vi får brug for det.
- ✅ Fejl følger HTTP-standarden (ADR 0011).
- ⚠️ Nye skærme kræver ofte et nyt endpoint eller en ny DTO. Med GraphQL kunne klienten bare ændre sin forespørgsel.
- ⚠️ Et skærmbillede kan kræve flere kald. Chatten henter f.eks. både `GET /api/chats/{id}` og `GET /api/chats/{id}/messages`.
- 🔁 Vi genovervejer, hvis der kommer flere klienter med forskellige behov (f.eks. en mobil-app): så GraphQL. Får vi brug for hurtige synkrone kald mellem services: så gRPC.
