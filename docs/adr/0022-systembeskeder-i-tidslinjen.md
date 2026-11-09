# 0022: Systembeskeder i tidslinjen – "Emma tilføjede Oliver" er en besked

Status: Accepteret (2026-11-09)

## Kontekst
Tidslinjen i en chat viser kun beskeder. Men der sker andre ting i en chat, som deltagerne bør kunne se, der hvor de skete:
- nogen bliver tilføjet ([0019](0019-forlad-chat-og-medlemsperiode-som-value-object.md) har tilføj deltager)
- nogen forlader chatten ([0019](0019-forlad-chat-og-medlemsperiode-som-value-object.md))
- chatten bliver omdøbt ([0020](0020-rest-api-crud-og-statuskoder.md))
- senere måske: gruppebilledet ændres, en besked fastgøres osv.

Vi ved i dag heller ikke, *hvem* der tilføjede nogen.

Alternativer:
- **A: Systembeskeder i `message`.** En ny kolonne `kind` (`user` eller `system`). Når noget sker, indsættes også en række i `message`.
- **B: Udled tidslinjen ved læsning.** `participation` har allerede `joined_at` og `left_at` (`MembershipPeriod`), så læsningen kunne lave en `UNION` af beskeder og medlemsændringer. Ingen ny data – men keyset paging ([0014](0014-beskeder-hentes-med-keyset-paging.md)) over to tabeller er besværlig og tungere på den varmeste sti, og omdøbninger og "hvem gjorde det" findes ikke nogen steder.
- **C: Frontenden fletter medlemmer ind.** Fungerer ikke med paging: frontenden ved ikke, hvilke medlemsændringer der hører til den side, den har hentet.

## Beslutning
Vi vælger **A: systembeskeder i `message`**.

**Schema:**
- `kind text not null default 'user'` – `user` (en person skrev) eller `system` (noget skete i chatten).
- `system_type text null` – kun for systembeskeder: `member_added`, `member_left`, `chat_renamed` (flere kan komme til).
- `details jsonb null` – data om hændelsen, fx `{ "userId": "..." }` eller `{ "oldName": "...", "newName": "..." }`.
- `sender_id` er den, der *gjorde* det (en participation, som ved almindelige beskeder). Så virker joins og navne uændret.

**Vi gemmer data, ikke færdig tekst.** `content` er tom for systembeskeder. Frontenden laver teksten ("Emma omdøbte chatten til …") ud fra `system_type` og `details`. Så kan teksten ændres eller oversættes, uden at gamle rækker skal opdateres. `jsonb` gør, at en ny type ikke kræver en ny kolonne.

**Handleren laver systembeskeden.** `Message` har én metode per hændelse (`Message.MemberAdded`, `MemberLeft`, `ChatRenamed`). Handlerne for tilføj deltager, forlad chat og omdøb chat gemmer først ændringen og derefter systembeskeden med `IMessageWriteRepository.AddAsync`. Det koster én ekstra `INSERT` ved en sjælden handling – læsningen af tidslinjen er stadig ét kald.

**Ikke i samme transaction.** De to repositories har hver sin forbindelse. Fejler systembeskeden, er ændringen gemt, men der mangler en linje i tidslinjen. Det er træls, men ikke farligt, så vi accepterer det frem for at indføre et nyt mønster nu. Skal det være alt-eller-intet senere, er løsningen **Unit of Work** (én forbindelse og transaction, som begge repositories bruger).

**Hvem er afsender?**
- `member_added`: den nye deltager, fordi `AddParticipantRequest` ikke siger, hvem der tilføjede ("Oliver blev tilføjet"). Kommer der et `AddedByUserId`, kan det blive "Emma tilføjede Oliver".
- `member_left`: den, der forlod chatten.
- `chat_renamed`: den, der omdøbte. Omdøbes til samme navn, laves ingen systembesked (intet skete).

**Regler:**
- Systembeskeder kan ikke slettes (`Message.Delete` afviser dem med samme 403 som andre).
- Kontrakten: `ChatMessageDto` får `Kind`, `SystemType` og `Details`. Previews i chatlisten viser systembeskeder som tekst ("Pia omdøbte chatten").

**Ingen events på RabbitMQ endnu.** De samme hændelser kan senere publiceres (`ChatRenamed`, `ParticipantAdded` …), når en service har brug for dem (YAGNI, som i [0020](0020-rest-api-crud-og-statuskoder.md)).

## Konsekvenser
- ✅ Tidslinjen er stadig én forespørgsel, og keyset paging virker uændret.
- ✅ Vi ved, hvem der omdøbte chatten (og hvad den hed før).
- ⚠️ Vi ved endnu ikke, hvem der tilføjede hvem (se ovenfor). Desuden tjekker tilføj deltager ikke, at den, der tilføjer, selv er medlem – et kendt hul som i [0013](0013-adgang-til-chats-og-mindst-mulig-information.md).
- ⚠️ Ændring og systembesked er ikke i samme transaction (se ovenfor).
- ✅ Nye typer (gruppebillede osv.) kræver kun en ny `system_type` og tekst i frontenden – ingen ny kolonne.
- ⚠️ Kræver ændringer i schemaet og i kontrakten. Lokalt skal databasen nulstilles (ingen migrations).
- ⚠️ Medlemskab står to steder: den nuværende tilstand i `participation` og historikken i `message`. Det er bevidst: historikken er en kendsgerning, der ikke ændrer sig.
- ⚠️ `details` er ikke tjekket af databasen (ingen fremmednøgler i `jsonb`). Formen styres af koden for hver `system_type`.
- ℹ️ Seedet har systembeskeder i chat 10: Rasmus omdøber chatten, og Kasper forlader den.
- ℹ️ Teksten laves i frontenden (`SystemMessageText`). En type, frontenden ikke kender, vises som "X ændrede noget i chatten" i stedet for at fejle.
- ℹ️ **Fremtid: vedhæftninger** (billeder, gifs, video). `kind` handler om *hvem der står bag*, ikke om indholdet, så vi bruger ikke `kind = 'image'` – en besked kan have tekst og flere billeder. Vedhæftninger får sin egen tabel (`message_attachment`), og selve filerne ligger i object storage (fx MinIO), ikke i Postgres. Sletning ([0021](0021-hvad-sletning-af-en-besked-betyder.md)) skal så også slette filerne. Det kræver sin egen ADR.
