# 0014: Beskeder hentes i sider med keyset paging

Status: Accepteret (2026-11-07)

## Kontekst
En chat kan få tusindvis af beskeder. Vi kan ikke hente dem alle, hver gang chatten åbnes. Klienten skal have de nyeste og selv kunne bede om ældre.

Alternativer:
- **A: Offset paging** (`?page=3&size=30`, i SQL `OFFSET 60 LIMIT 30`). Det er simpelt, men:
  - Det skrider, når der kommer nye beskeder. Hver ny besked skubber alt én plads, så side 2 viser nogle af de samme beskeder igen eller springer nogle over.
  - Det bliver langsommere, jo længere tilbage man bladrer, fordi databasen skal læse og smide alle rækkerne før offset væk.
- **B: Keyset paging (cursor)** (`?before=<id på den ældste besked, man har>&limit=30`). Man spørger "de 30 nyeste, der er ældre end X". Det er det, Slack og Discord gør.

## Beslutning
- Vi vælger **B**: `GET /api/chats/{chatId}/messages?userId=...&before=...&limit=...`.
- **Standard er 30 beskeder.** Det svarer til et par skærmfulde. Klienten må bede om 1–100. Andet giver `400`.
- Brugeren **beder selv** om ældre beskeder ("Vis ældre beskeder"). Vi henter ikke automatisk.
- **Cursoren er en beskeds id**, ikke et tidspunkt. "Ældre end X" er `(sent_at, id) < (X.sent_at, X.id)`. id er med, så to beskeder med præcis samme tidspunkt hverken springes over eller kommer med to gange.
- Vi henter `limit + 1` rækker. Kommer den ekstra med, findes der ældre beskeder (`hasOlder: true`), og vi slipper for en ekstra `COUNT`.
- Svaret har den **ældste først**, så klienten kan vise dem oppefra og ned.
- Index `ix_message_chat_sent` på `(chat_id, sent_at desc, id desc)` gør hver side til ét opslag i indexet.
- Kun medlemmer må læse (ADR 0013).

## Konsekvenser
- ✅ Stabilt, selvom der kommer nye beskeder imens. Passer til realtid: nye beskeder lægges bare i bunden.
- ✅ Lige hurtigt på side 1 og side 100.
- ✅ Indexet hjælper også "seneste besked" i listen over mine chats (LATERAL).
- ⚠️ Man kan ikke hoppe direkte til "side 7". Det har en chat heller ikke brug for.
- ⚠️ Indexet koster lidt ved hver INSERT. Det er ubetydeligt ved vores mængder.
- ⚠️ Indexet ligger i `01-schema.sql`, som kun køres på en tom database. En eksisterende database skal have det tilføjet manuelt (eller nulstilles med `docker compose down -v`).
