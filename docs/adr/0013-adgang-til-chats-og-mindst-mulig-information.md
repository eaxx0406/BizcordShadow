# 0013: Kun medlemmer har adgang til en chat – og fejl afslører mindst muligt

Status: Accepteret (2026-11-07)

## Kontekst
Nu kan man sende beskeder (`POST /api/chats/{chatId}/messages`). Kun medlemmer af chatten må skrive i den.

Vi har ingen rigtig login, fordi UserService er uden for scope (ADR 0003). Klienten sender selv sit user-id, og ChatService **tager det for gode varer**. Sikkerhed er ikke fokus dette semester, men vi vil gerne have tænkt tanken, så designet er klar til rigtig login senere.

Når en bruger ikke må skrive i en chat, kan der være to grunde: chatten findes ikke, eller brugeren er ikke medlem. Hvad skal klienten have at vide?

Alternativer:
- **A: Skeln mellem dem.** `404` hvis chatten ikke findes, `403` hvis man ikke er medlem. Det er nemt at fejlsøge, men en angriber kan bruge det til at finde ud af, hvilke chat-id'er der findes (enumeration).
- **B: Samme svar i begge tilfælde.** Klienten får aldrig at vide, om chatten findes. Princippet er **mindst mulig information**: et fejlsvar må ikke fortælle en uvedkommende mere, end de har brug for.
  - **B1: Altid `404`** (som GitHub gør med private repositories): "for dig findes chatten ikke".
  - **B2: Altid `403`**: "du har ikke adgang til denne chat".

## Beslutning
- Vi vælger **B2: `403 Forbidden`**, både når chatten ikke findes, og når man ikke er medlem. Svaret har samme besked i begge tilfælde: "Du har ikke adgang til denne chat."
- Handleren laver ét opslag: brugerens **aktive** participation i chatten. Findes den ikke, kastes en `ForbiddenException`, som den globale exception handler (ADR 0011) oversætter til `403` med ProblemDetails. Den udvider dermed tabellen i ADR 0011 med en ny række: "ikke adgang til chatten → 403".
- Den præcise grund (findes ikke eller ikke medlem) står **aldrig i svaret**. Når vi får logging, skrives den i loggen.
- user-id'et fra klienten stoles på, indtil der findes login. Med login kommer user-id'et fra login-tokenet (`ICurrentUser`), og så forsvinder `senderUserId` fra requesten. Reglen ovenfor ændrer sig ikke.

## Konsekvenser
- ✅ Et fejlsvar på at sende en besked afslører ikke, om en chat findes.
- ✅ Ét opslag i databasen besvarer både "findes chatten?" og "er du medlem?".
- ✅ Når rigtig login kommer, skal kun kilden til user-id'et ændres, ikke reglen.
- ⚠️ Sværere at fejlsøge for klienten, og indtil vi får logging, står den rigtige grund ingen steder.
- ⚠️ **Kendt hul:** `GET /api/chats/{id}` returnerer i dag chatten til alle og svarer `404` for en ukendt chat. Den afslører altså både at chatten findes, og hvem der er med. Vi accepterer det, indtil der findes identitet. Så bliver læsning også kun for medlemmer med samme regel (`403`).
- ⚠️ **Kendt hul:** Uden login kan alle udgive sig for at være hvem som helst ved at sende et andet user-id. Det er bevidst fravalgt dette semester.
- ℹ️ Chat-id'er er tilfældige GUID'er, så de er i praksis umulige at gætte. Princippet om mindst mulig information gælder alligevel, fordi id'er kan lække (logs, delte links, skærmbilleder).
