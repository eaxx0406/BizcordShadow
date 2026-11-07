# 0010: Chat-aggregatet styrer deltagelse – maks 50 deltagere

Status: Accepteret (2026-11-07) – endnu ikke implementeret

## Kontekst
Medlemmer af en chat skal kunne tilføje andre brugere til den. Der gælder tre regler:

1. En bruger kan kun have ét aktivt medlemskab af samme chat.
2. En chat har højst **50** aktive deltagere.
3. Kun brugere, der selv er aktive medlemmer af chatten, må tilføje andre. Det er ikke kun opretteren, der må.

Spørgsmålet er, hvem der håndhæver reglerne.

Alternativer:
- **A: `Chat` er aggregate root og ejer sine participations.** For at tilføje en deltager loades chatten med sine aktive participations, reglerne tjekkes i `Chat`, og den nye participation gemmes.
- **B: Participation er sit eget aggregate.** Hver tilføjelse er én insert, og databasen håndhæver reglerne.

B skalerer bedre ved meget store chats (tusindvis af medlemmer), fordi man ikke skal loade alle medlemmer. Men regel 2 (loftet) og regel 3 (kun medlemmer) kan ikke udtrykkes som en database-constraint. Man skal kende chattens medlemmer for at tjekke dem. Med et loft på 50 koster det kun én forespørgsel efter højst 50 små rækker via et index at loade dem. Antallet af chats i alt påvirker ikke prisen.

## Beslutning
- Vi vælger **A**. `Chat` er aggregate root, og alle ændringer i deltagelse sker gennem metoder på `Chat`, som håndhæver regel 1–3.
- Loftet på 50 er en regel i domænet (på `Chat`), ikke i databasen.
- Når en chat loades for at blive ændret, hentes kun de **aktive** participations (`left_at is null`). Det dækkes af indexet `ux_participation_active`.
- Da Dapper ikke har change tracking, gemmer repository'et kun den participation, der er tilføjet. Det overskriver ikke hele aggregatet.
- `ux_participation_active` bliver i databasen som sikkerhedsnet. Hvis to requests tilføjer samme bruger samtidig, fanger indexet det, og fejlen oversættes til "allerede medlem" (se [0011](0011-fejlhaandtering-med-problemdetails.md)).
- Endpoint: `POST /api/chats/{chatId}/participants` med `{ "userId": "..." }` i bodyen. Svaret er `201 Created`. Bodyen indeholder et **user-id**, fordi klienter kender brugere og ikke participants. ChatService slår selv participant op (se [0003](0003-userservice-er-source-of-truth.md)).
- Gatewayen får en catch-all-route for `/api/chats/{everything}`, så nye under-stier ikke kræver en ny route hver gang.

## Konsekvenser
- ✅ Alle regler om deltagelse står ét sted og kan testes uden database.
- ✅ Svarer til DDD: roden beskytter reglerne for sine børn.
- ✅ Loftet holder prisen for at loade en chat lav og forudsigelig.
- ⚠️ `Chat` skal kunne **genopbygges** fra databasen uden at gå gennem konstruktøren, som opretter en ny chat med nyt id.
- ⚠️ Repository'et skal vide, hvilken participation der er ny.
- ⚠️ **Kendt race condition:** to samtidige requests på en chat med 49 deltagere kan begge se 49 og ende med 51. Det accepterer vi indtil videre. Løsningen er optimistic concurrency (en `version`-kolonne på `chat`) eller at låse chat-rækken (`SELECT ... FOR UPDATE`).
- ⚠️ Regel 3 kræver, at ChatService ved, hvem der kalder. Indtil der findes en abstraktion for den aktuelle bruger, kan reglen ikke håndhæves fuldt ud.
- ⚠️ Skal chats senere kunne have tusindvis af medlemmer, skal beslutningen tages op igen (alternativ B).
