# 0007: Contracts-projekt per service

Status: Accepteret (2026-11-06)

## Kontekst
En service' API består af request- og response-typer (DTO'er). Andre projekter (frontend, gateway, andre services) har brug for de samme typer. Ligger de inde i service-projektet, må andre projekter referere hele servicen – inklusive domænemodel og infrastruktur.

Alternativer:
- **DTO'er i service-projektet:** simpelt, men andre må enten referere hele servicen eller kopiere typerne.
- **Ét fælles DTO-projekt for alle services:** let at dele, men kobler alle services sammen.
- **Et contracts-projekt per service:** kun servicens offentlige API-typer.

## Beslutning
Hver service har et tilhørende `<Service>.Contracts`-projekt med dens offentlige DTO'er, f.eks. `ChatService.Contracts` med `CreateChatRequest` og `ChatDto`. Contracts-projektet må ikke referere servicens interne kode (entities, repositories).

## Konsekvenser
- ✅ Klar grænse mellem servicens offentlige API og dens interne model.
- ✅ Andre projekter kan bruge DTO'erne uden at afhænge af servicens implementering.
- ✅ Ændringer i domænemodellen bryder ikke klienter, så længe kontrakten er den samme.
- ⚠️ Flere projekter i solution.
- ⚠️ Mapping mellem entity og DTO skal skrives.
- ⚠️ Frontend har pt. sin egen kopi af `CreateChatRequest` i `GatewayClient.cs` i stedet for at bruge `ChatService.Contracts`.
