# 0009: Ingen delt domænekode mellem services

Status: Accepteret (2026-11-06)

## Kontekst
Vi havde et `Shared`-projekt med generiske repository-interfaces (`ICreateRepository<T>`, `IReadRepository<T>`), som services skulle arve fra. Formålet var genbrug (DRY).

Men i microservices er delt kode en form for kobling: ændres `Shared`, skal alle services, der refererer det, bygges, testes og deployes igen. De generiske interfaces gav desuden kun lidt værdi – ingen kode brugte dem generisk, og hver service har brug for egne metoder (f.eks. `AddParticipantAsync`), der ikke passer i en generisk skabelon.

Alternativer:
- **Behold `Shared` med generiske interfaces:** mindre duplikering, men services kobles sammen.
- **Hver service ejer sine egne interfaces:** lidt duplikering, men fuld uafhængighed.

## Beslutning
- `Shared` er fjernet. Hver service definerer selv sine repository-interfaces, skræddersyet til dens behov (f.eks. `IChatWriteRepository` i ChatService).
- **Kontrakter** (DTO'er og integration events) ejes af den service, der udgiver dem, og ligger i dens `<Service>.Contracts`-projekt (se [0007](0007-contracts-projekt-per-service.md)). Eksempel: `UserCreated` vil ligge i `UserService.Contracts`.
- **Teknisk infrastruktur** uden forretningsviden (logging, health checks, message broker-opsætning) må gerne deles i et fælles projekt (f.eks. `ServiceDefaults`), hvis behovet opstår.

Tommelfingerregel: **del infrastruktur – aldrig domæne.**

## Konsekvenser
- ✅ Services kan ændres og deployes uafhængigt.
- ✅ Interfaces beskriver præcis det, servicen har brug for.
- ✅ Klart ejerskab af kontrakter.
- ⚠️ Noget kode vil ligne hinanden på tværs af services. Det accepterer vi bevidst: duplikering er billigere end kobling.
- ⚠️ Kræver disciplin: et nyt fælles projekt må kun indeholde infrastruktur.
