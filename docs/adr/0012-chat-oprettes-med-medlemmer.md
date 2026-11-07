# 0012: En chat oprettes med medlemmer i ét kald

Status: Accepteret (2026-11-07)

## Kontekst
Indtil nu blev en chat oprettet med kun opretteren som medlem. Andre blev tilføjet bagefter, én ad gangen, med `POST /api/chats/{chatId}/participants` (ADR 0010). For brugeren var det to trin, og en chat kun med en selv er ikke interessant.

Alternativer:
- **A: Frontend opretter chatten og kalder derefter add participant for hvert medlem.** Kræver ingen ændring i ChatService. Men det er flere kald, og fejler et af dem, står der en halv chat tilbage.
- **B: Oprettelsen tager medlemmerne med.** `POST /api/chats` får en liste af user-id'er, og chatten gemmes med alle deltagere i én transaktion.

## Beslutning
- Vi vælger **B**. `CreateChatRequest` får `ParticipantUserIds` (user-id'er, ikke participant-id'er, se ADR 0003).
- En chat skal have **mindst ét medlem ud over opretteren**. Det er en regel i domænet: `Chat.Create(...)` er nu den eneste måde at lave en ny chat på, og konstruktøren er private. Controlleren tjekker det også, så klienten får `400` i stedet for `500`.
- Medlemmerne tilføjes inde i `Chat.Create` gennem `AddParticipant`. Reglerne fra ADR 0010 (ingen dubletter, maks 50) gælder derfor også ved oprettelse.
- `POST /api/chats/{chatId}/participants` bliver. At tilføje deltagere til en eksisterende chat er stadig en use case.

## Konsekvenser
- ✅ Én handling for brugeren og ét kald til ChatService.
- ✅ Alt-eller-intet: findes ét af medlemmerne ikke, oprettes chatten slet ikke.
- ✅ Samme regler for medlemmer, uanset om de kommer med ved oprettelsen eller senere.
- ⚠️ `CreateChatRequest` er ændret. Klienter, der sender det gamle format uden `participantUserIds`, får `400`.
- ⚠️ Handleren slår hvert medlem op for sig. Det er fint med få medlemmer, men ved store grupper bør opslaget samles i én forespørgsel.
