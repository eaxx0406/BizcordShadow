# 0020: REST API med CRUD – omdøb chat, slet besked og faste regler for statuskoder

Status: Accepteret (2026-11-08)

## Kontekst
Moodle (uge 38, lab 3) beder om et REST API med CRUD (POST, GET én og flere, PUT og DELETE), hvor forretningslogik er adskilt fra præsentation, input valideres, dependency injection bruges, og HTTP-metoder og statuskoder bruges konsekvent.

Vi havde POST, GET og DELETE (forlad chat, [0019](0019-forlad-chat-og-medlemsperiode-som-value-object.md)), men ingen PUT. Desuden var API'et inkonsistent:
- `POST /api/chats` og send besked svarede `200 OK` i stedet for `201 Created`.
- Nogle 400-fejl var ren tekst (`return BadRequest("...")` i controlleren), mens resten var ProblemDetails ([0011](0011-fejlhaandtering-med-problemdetails.md)).
- "Mindst ét medlem ud over opretteren" blev tjekket i controlleren, og domænets guard kastede `InvalidOperationException`, som ville være blevet til `500`.

To nye funktioner giver mening i en chat: **omdøb chat** (PUT) og **slet besked** (DELETE).

Alternativer for slet besked:
- **A: Hard delete.** Rækken slettes. Ingen ændring i schemaet, men en klient, der pager med `before=<slettet id>` ([0014](0014-beskeder-hentes-med-keyset-paging.md)), får en fejl, og `message_receipt` skal ryddes op senere.
- **B: Soft delete.** Ny kolonne `deleted_at`. Indholdet tømmes, men rækken bliver, og chatten viser "Beskeden er slettet".

## Beslutning
**Omdøb chat:** `PUT /api/chats/{chatId}` med `{ userId, name }` → `204`.
- Alle **aktive medlemmer** må omdøbe. Vi har ingen roller (admin/ejer). Ikke-medlemmer får `403` ([0013](0013-adgang-til-chats-og-mindst-mulig-information.md)).
- Reglen for navne ligger i `Chat` og gælder både ved oprettelse og omdøbning: trimmes, må ikke være tomt, højst 100 tegn.
- PUT og ikke PATCH: navnet er det eneste, der kan ændres på en chat, så PUT erstatter hele den redigerbare del.

**Slet besked:** `DELETE /api/chats/{chatId}/messages/{messageId}?userId=` → `204`.
- Vi vælger **B: soft delete**. Indholdet tømmes, og `deleted_at` sættes. Teksten er væk for altid, men rækken bliver, så paging og samtalens forløb hænger sammen. Hvorfor teksten ikke arkiveres, står i [0021](0021-hvad-sletning-af-en-besked-betyder.md).
- **Kun afsenderen må slette.** Det er den eneste regel: er det din besked, må du slette den, uanset om du stadig er med i chatten.
- **Reglen ligger i `Message.Delete`.** Handleren henter beskeden (med afsenderens user-id), `Message` afgør, om brugeren må slette den, og handleren gemmer. Det er to kald til databasen, men alle regler for sletning er samlet ét sted, når de udvides (fx med en moderator).
- **Én fejl for alt:** `403` "Beskeden kunne ikke slettes", både når beskeden ikke findes, og når den ikke er din. Vi fortæller ikke hvorfor (mindst mulig information, [0013](0013-adgang-til-chats-og-mindst-mulig-information.md)), og vi slår ikke op, om man er medlem.
- DELETE er **idempotent**: at slette sin egen, allerede slettede besked giver også `204` (det første tidspunkt bevares).
- `userId` står i query-strengen, ikke i en body, fordi DELETE med body ikke understøttes alle steder.
- Kontrakterne viser det udadtil med `ChatMessageDto.IsDeleted`. I listen over chats er `LastMessage` `null`, hvis den seneste besked er slettet.

**Rediger besked:** `PUT /api/chats/{chatId}/messages/{messageId}` med `{ userId, content }` → `204`.
- Reglerne ligger i `Message.Edit`, ved siden af `Delete`: kun afsenderen, kun almindelige beskeder, ikke slettede.
- **Kun i 15 minutter efter afsendelse** (`Message.EditWindow`), som i de fleste chat-apps. Efter det: `409` med grunden – det er ens egen besked, så vi må gerne sige hvorfor.
- Ikke din, findes ikke, slettet eller systembesked: `403` "Beskeden kunne ikke redigeres" (samme princip som slet).
- `edited_at` sættes, og `ChatMessageDto.IsEdited` får klienten til at vise "(redigeret)". Den gamle tekst gemmes ikke ([0021](0021-hvad-sletning-af-en-besked-betyder.md)).
- Seed-beskederne er alle ældre end 15 minutter, så de kan ikke rettes – det er reglen, der virker. Send en ny besked for at prøve.

**Ingen events endnu.** Hverken `ChatRenamed` eller `MessageDeleted` publiceres (YAGNI: ingen service bruger dem).

**Faste regler for hele API'et:**

| Situation | Svar |
|---|---|
| Noget nyt er oprettet, og det har en URL | `201 Created` + `Location` + den nye ressource (`POST /api/chats`) |
| Noget nyt er oprettet, men det har ingen egen URL | `201 Created` + den nye ressource, uden `Location` (send besked) |
| Det lykkedes, og der er intet at sende tilbage | `204 No Content` (PUT, DELETE, tilføj deltager) |
| Læsning | `200 OK` |
| Ugyldigt input | `400` med ProblemDetails, enten fra attributter på kontrakterne eller fra `DomainValidationException` |
| Ikke medlem / ingen adgang | `403` ([0013](0013-adgang-til-chats-og-mindst-mulig-information.md)) |
| Findes ikke (når man har adgang) | `404` |
| Konflikt med en regel | `409` |

- **Validering af input** sker med attributter på kontrakterne (`[Required]`, `[MaxLength]`, `[MinLength]`) og den nye `[NotEmptyGuid]` (fordi `[Required]` ikke afviser `Guid.Empty`). `[NotEmptyGuid]` bruges også direkte på parametre som `?userId=`. `[ApiController]` svarer så selv `400`, før controlleren kaldes. Controllerne indeholder ingen `if`-tjek længere.
- **Guards i domænet** kaster `DomainValidationException` → `400` (ny række i [0011](0011-fejlhaandtering-med-problemdetails.md)). Det gælder bl.a. "mindst ét medlem ud over opretteren" og navnereglen.

**Endpoints i ChatService efter denne ADR:**

| Metode | URL | Svar |
|---|---|---|
| GET | `/api/chats?userId=` | 200 – mine chats |
| GET | `/api/chats/{chatId}?userId=` | 200 – én chat med medlemmer |
| POST | `/api/chats` | 201 – opret chat |
| PUT | `/api/chats/{chatId}` | 204 – omdøb chat |
| POST | `/api/chats/{chatId}/participants` | 204 – tilføj deltager |
| DELETE | `/api/chats/{chatId}/participants/{userId}` | 204 – forlad chat |
| GET | `/api/chats/{chatId}/messages?userId=&before=&limit=` | 200 – én side beskeder |
| POST | `/api/chats/{chatId}/messages` | 201 – send besked |
| PUT | `/api/chats/{chatId}/messages/{messageId}` | 204 – ret egen besked (15 min) |
| DELETE | `/api/chats/{chatId}/messages/{messageId}?userId=` | 204 – slet egen besked |

Hver route er også oprettet i Ocelot ([0001](0001-api-gateway-med-ocelot.md)).

## Konsekvenser
- ✅ Alle CRUD-operationer findes, og alle fejl har samme format (ProblemDetails). Beskeder har fuld CRUD: send (POST), hent (GET), ret (PUT) og slet (DELETE).
- ⚠️ 15-minutters-reglen bruger `DateTimeOffset.UtcNow` direkte, så en unit test af reglen skal flytte `SentAt` (fx via `Rehydrate`). Bliver det besværligt, kan uret sendes ind med .NET's `TimeProvider`.
- ✅ Controllerne er tynde: de oversætter kun HTTP til commands/queries og vælger statuskode. Navnereglen ligger i `Chat`.
- ✅ Reglerne for, hvem der må slette, er samlet i `Message.Delete` og kan unit-testes uden database.
- ✅ Slettede beskeder ødelægger ikke keyset paging, og samtalen giver stadig mening ("Beskeden er slettet").
- ✅ `[NotEmptyGuid]` ligger i `ChatService.Contracts`, så klienter kan validere med samme regel.
- ⚠️ Kontrakten er ændret: `ChatMessageDto` har fået `IsDeleted`, og flere request-typer har fået valideringsattributter. Det rammer frontenden, men ingen andre services.
- ⚠️ Kræver `deleted_at` i schemaet. Lokalt skal databasen nulstilles (`docker compose down -v`), fordi vi ikke har migrations.
- ⚠️ **`MessageDeleted` mangler.** Andre services har en kopi af indholdet fra `MessageSent` ([0017](0017-modtagere-sendes-med-i-messagesent.md)). Når en service viser beskeder eller previews (notifikationer, realtime), skal ChatService publicere `MessageDeleted`. Ellers bliver en slettet besked ved med at kunne ses der.
- ⚠️ Slet besked koster to kald (hent + gem). Vi har overvejet ét `UPDATE` med reglen i `WHERE`, men valgt at samle reglen i domænet. Sletning er sjælden, så det ekstra kald er billigt.
- ℹ️ `Message` har fået `SenderUserId` (hentes via join, gemmes ikke i `message`), fordi `SenderId` er et participation-id. Afsenderen sammenlignes på user-id, så man kan også slette sine beskeder fra et tidligere medlemskab eller efter at have forladt chatten.
- ⚠️ Kendt hul (som [0013](0013-adgang-til-chats-og-mindst-mulig-information.md)): uden login kan man omdøbe eller slette på vegne af en anden ved at sende deres user-id.
