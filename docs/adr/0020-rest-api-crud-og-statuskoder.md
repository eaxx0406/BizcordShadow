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
- Vi vælger **B: soft delete**. `Message.Delete` tømmer `Content` og sætter `DeletedAt`. Teksten er væk for altid, men rækken bliver, så paging og samtalens forløb hænger sammen.
- Kun **afsenderen** må slette, og kun som aktivt medlem. Andre medlemmer får `403` ("Du kan kun slette dine egne beskeder"). Ikke-medlemmer får `403` som altid. En besked, der ikke findes, giver `404`. Det afslører intet, fordi man allerede er medlem.
- DELETE er **idempotent**: at slette en allerede slettet besked giver også `204`.
- `userId` står i query-strengen, ikke i en body, fordi DELETE med body ikke understøttes alle steder.
- Kontrakterne viser det udadtil med `ChatMessageDto.IsDeleted`. I listen over chats er `LastMessage` `null`, hvis den seneste besked er slettet.

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
| DELETE | `/api/chats/{chatId}/messages/{messageId}?userId=` | 204 – slet egen besked |

Hver route er også oprettet i Ocelot ([0001](0001-api-gateway-med-ocelot.md)).

## Konsekvenser
- ✅ Alle CRUD-operationer findes, og alle fejl har samme format (ProblemDetails).
- ✅ Controllerne er tynde: de oversætter kun HTTP til commands/queries og vælger statuskode. Reglerne ligger i `Chat` og `Message`.
- ✅ Slettede beskeder ødelægger ikke keyset paging, og samtalen giver stadig mening ("Beskeden er slettet").
- ✅ `[NotEmptyGuid]` ligger i `ChatService.Contracts`, så klienter kan validere med samme regel.
- ⚠️ Kontrakten er ændret: `ChatMessageDto` har fået `IsDeleted`, og flere request-typer har fået valideringsattributter. Det rammer frontenden, men ingen andre services.
- ⚠️ Kræver `deleted_at` i schemaet. Lokalt skal databasen nulstilles (`docker compose down -v`), fordi vi ikke har migrations.
- ⚠️ **`MessageDeleted` mangler.** Andre services har en kopi af indholdet fra `MessageSent` ([0017](0017-modtagere-sendes-med-i-messagesent.md)). Når en service viser beskeder eller previews (notifikationer, realtime), skal ChatService publicere `MessageDeleted`. Ellers bliver en slettet besked ved med at kunne ses der.
- ⚠️ Afsenderen sammenlignes på medlemskab (participation), ikke på person. Har man forladt chatten og er blevet tilføjet igen, kan man ikke slette beskeder fra sit gamle medlemskab. Det accepterer vi.
- ⚠️ Kendt hul (som [0013](0013-adgang-til-chats-og-mindst-mulig-information.md)): uden login kan man omdøbe eller slette på vegne af en anden ved at sende deres user-id.
