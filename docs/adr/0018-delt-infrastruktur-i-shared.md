# 0018: Delt infrastruktur ligger i shared/ – MessageClient er den første

Status: Foreslået (2026-11-08)

## Kontekst
`MessageClient` (publish/subscribe via RabbitMQ) blev bygget i `apps/chat-microservice/src`, fordi ChatService var den første, der skulle bruge den. Men den kender intet til chats, kun "publish en T" og "subscribe på en T". Da RealTimeCommunicationService også skulle bruge den, måtte den referere ind i chat-mappen (`../../../chat-microservice/src/MessageClient`). Det ser ud, som om RTC afhænger af ChatService, selvom den kun afhænger af et transportlag.

[ADR 0009](0009-ingen-delt-domaenekode-mellem-services.md) fjernede det gamle `Shared`-projekt, fordi det delte domænekode, men tillader delt teknisk infrastruktur.

Alternativer:
- **A: Lad den ligge hos chat.** Ingen flytning, men forkert ejerskab, og andre services refererer ind i chat.
- **B: Kopiér den ind i hver service.** Fuld uafhængighed, men samme fejl skal rettes flere steder.
- **C: Et fælles `shared/`-område i roden** med project references.
- **D: En NuGet-pakke** (fx `Bizcord.MessageClient`) i et privat feed. Hver service vælger selv version.

## Beslutning
- Vi vælger **C**. Fælles infrastruktur ligger i `shared/src/<Projekt>`. `MessageClient` er det første projekt.
- `shared/` må kun indeholde infrastruktur uden forretningsviden. Domænekode og kontrakter hører ikke til her (ADR 0007, 0009).
- Der er **ét** MessageClient-bibliotek. Hver service laver sin egen instans ved opstart (`AddMessageClient(...)` med sin egen connection string).

## Konsekvenser
- ✅ Klart ejerskab: services afhænger af `shared`, ikke af hinanden.
- ✅ Én implementering at rette og forbedre.
- ⚠️ En ændring i `shared` rammer alle services, der bruger den, på én gang (ingen versionering). Det er acceptabelt i en monorepo med én gruppe.
- ⚠️ Dockerfiler skal kopiere `shared/src/` med og bygges fra roden (som i ADR 0016).
- ⚠️ Ejes services af forskellige teams eller repos, skal vi over til **D** (NuGet med versioner).
- ⚠️ Services i andre sprog kan ikke bruge biblioteket. De taler direkte med RabbitMQ og følger samme JSON-format.
