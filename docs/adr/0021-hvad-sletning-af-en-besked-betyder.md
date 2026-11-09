# 0021: Hvad sletning af en besked betyder – teksten slettes rigtigt, arkivering er fravalgt

Status: Accepteret (2026-11-09)

## Kontekst
[0020](0020-rest-api-crud-og-statuskoder.md) indførte "slet besked" som soft delete. Men hvad *betyder* det, når en bruger trykker slet? Er teksten væk, eller ligger den gemt et sted?

Spørgsmål, vi har overvejet:
- **Bevis.** Hvis nogen skriver noget kriminelt og sletter det, skal teksten så kunne findes igen (fx til en retssag)?
- **Følsomt indhold.** Hvis nogen ved en fejl skriver en adgangskode og sletter den, må teksten ikke kunne findes af andre – heller ikke af nogen med "computer skills".
- **Tidslinjen.** Samtalen skal stadig give mening ("Kasper slettede en besked"), og keyset paging ([0014](0014-beskeder-hentes-med-keyset-paging.md)) må ikke gå i stykker.
- **Performance.** Hvad koster de forskellige løsninger, når systemet vokser?

Alternativer:
- **A: Skjul kun i GUI'en.** Teksten bliver i databasen og i API-svaret, men frontenden viser den ikke. Fravalgt: alle kan se den med F12, og den ligger stadig i databasen, backups og logs.
- **B: Flyt beskeden til en `archived_message`-tabel.** Teksten gemmes til bevis, men hentes ikke med, når chatten vises. Fravalgt, se nedenfor.
- **C: Normal sletning.** Rækken bliver, men teksten overskrives (`content = ''`) og `deleted_at` sættes.
- **D: Legal hold.** Normal sletning, men hvis en konto eller chat er "frosset" (fx efter et pålæg fra politiet), arkiveres teksten i stedet. Udskudt, se nedenfor.

## Beslutning
Vi vælger **C: normal sletning**. Når en bruger sletter sin besked, er teksten **væk for altid** i ChatService.

**Hvorfor ikke arkivering (B)?**
- I EU må man ikke gemme alt indhold for alle, "hvis nu nogen begår en forbrydelse". EU-Domstolen har flere gange underkendt generel logning, og det var endda kun metadata.
- GDPR (artikel 17): brugeren har ret til sletning. Et arkiv, der gemmer al slettet tekst, ville kræve en lovlig grund og en regel for, hvor længe den gemmes.
- Det løser ikke det kriminelle problem alligevel: bevis findes typisk på telefonerne (modtagerens kopi, skærmbilleder, backups) og i metadata – ikke på platformens server.
- Et nyt koncept (en ekstra tabel, en ekstra læsesti, evt. en ekstra `Rehydrate`) uden en konkret bruger (YAGNI).

**Retten til sletning beskytter ikke kriminelle.** GDPR har undtagelser for retlige forpligtelser og retskrav (artikel 17(3)). Hvis politiet kræver data bevaret, må platformen ikke slette – det er det, legal hold (D) er til. Vi bygger det ikke nu, men designet udelukker det ikke.

**Hvordan:**
- `Message.Delete` afgør, om brugeren må slette ("det skal være din besked"), og sætter tom tekst og `DeletedAt`. Repository'et gemmer det med ét `UPDATE` ([0020](0020-rest-api-crud-og-statuskoder.md)).
- Rækken bliver med afsender og tidspunkt, så tidslinjen og paging virker.
- API'et sender `IsDeleted = true` og tom tekst, så klienten viser "Beskeden er slettet".
- `Message`-entiteten skal ikke kende til arkivering. Chat-domænet skal kun vide, *at* en besked er slettet.

## Konsekvenser
- ✅ Det brugeren sletter, er rigtigt slettet (adgangskoden er væk).
- ✅ Billigt: slettede rækker koster stort set intet i `message`. Sletninger er få, og paging bruger indekset `(chat_id, sent_at, id)`. At flytte rækker (B) ville koste DELETE + INSERT, indeks i to tabeller og oprydning (vacuum).
- ✅ Ingen ekstra tabel, læsesti eller `Rehydrate`.
- ⚠️ Teksten kan ikke findes igen – heller ikke af os, en moderator eller politiet.
- ⚠️ Kopier andre steder slettes ikke: modtagerens skærm, notifikationer og andre services, der har fået teksten via `MessageSent` ([0017](0017-modtagere-sendes-med-i-messagesent.md)). Se `MessageDeleted` i [0020](0020-rest-api-crud-og-statuskoder.md).
- ⚠️ Postgres overskriver ikke den gamle række fysisk med det samme (MVCC). Indtil vacuum har kørt, kan teksten i teorien ligge på disken, og den ligger i backups, indtil de udløber.
- ℹ️ Kommer der moderatorer/admins, udvides `Message.Delete`. Kommer der legal hold, kan arkivering tilføjes for frosne konti alene. Begge dele kræver en ny ADR.
