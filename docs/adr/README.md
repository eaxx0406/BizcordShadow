# Architecture Decision Records (ADR)

Her dokumenterer vi de vigtige arkitekturbeslutninger i Bizcord: hvad vi valgte, hvorfor, og hvad det koster.

## Regler
- Én beslutning per fil, nummereret fortløbende: `NNNN-kort-titel.md`.
- En accepteret ADR rettes ikke i indhold. Ændrer vi mening, skriver vi en **ny** ADR og sætter den gamles status til `Erstattet af NNNN`.
- Hold dem korte: Kontekst → Beslutning → Konsekvenser.

## Oversigt

| #    | Titel | Status |
|------|-------|--------|
| [0001](0001-api-gateway-med-ocelot.md) | API Gateway med Ocelot | Accepteret |
| [0002](0002-database-per-service.md) | Database per service | Accepteret |
| [0003](0003-userservice-er-source-of-truth.md) | UserService er source of truth – Participant er en read model | Accepteret |
| [0004](0004-cqrs-light-med-mediatr.md) | CQRS-light med MediatR | Accepteret |
| [0005](0005-dapper-og-database-first.md) | Dapper og database first | Accepteret |
| [0006](0006-postgres-navngivning-snake-case.md) | Postgres-navngivning med snake_case | Accepteret |
| [0007](0007-contracts-projekt-per-service.md) | Contracts-projekt per service | Accepteret |
| [0008](0008-lokal-infrastruktur-compose-og-user-secrets.md) | Lokal infrastruktur med docker compose og user-secrets | Accepteret |
| [0009](0009-ingen-delt-domaenekode-mellem-services.md) | Ingen delt domænekode mellem services | Accepteret |
| [0010](0010-chat-styrer-deltagelse.md) | Chat-aggregatet styrer deltagelse – maks 50 deltagere | Accepteret |
| [0011](0011-fejlhaandtering-med-problemdetails.md) | Fejlhåndtering med domæne-exceptions og ProblemDetails | Accepteret |

## Skabelon

```markdown
# NNNN: Titel

Status: Foreslået | Accepteret | Erstattet af NNNN (dato)

## Kontekst
Hvilket problem står vi med? Hvilke alternativer var der?

## Beslutning
Hvad vælger vi?

## Konsekvenser
Hvad bliver nemmere? Hvad bliver sværere / hvad skal vi leve med?
```
