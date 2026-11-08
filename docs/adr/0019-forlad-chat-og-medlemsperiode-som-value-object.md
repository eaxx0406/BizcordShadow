# 0019: Man kan forlade en chat – medlemsperioden er et value object

Status: Accepteret (2026-11-08)

## Kontekst
Indtil nu kunne man komme ind i en chat (oprettes med den eller blive tilføjet, se [0010](0010-chat-styrer-deltagelse.md) og [0012](0012-chat-oprettes-med-medlemmer.md)), men ikke ud igen. Databasen var allerede forberedt: `participation.left_at` og det unikke indeks `where left_at is null`, og alle læsninger filtrerer på aktive medlemskaber.

Samtidig beder Moodle (uge 38, lab 1) om en domænemodel med **entities** og **value objects**. Vi havde entities, men ingen value objects. Et value object har ingen identitet, er immutable og sammenlignes på værdi. Det giver kun mening, når værdien har **regler**. Ellers er det bare en indpakket `string` eller `Guid` (ceremoni).

Kandidater til value objects:
- **A: `ChatName` og `MessageContent`.** De har regler (ikke tom, max-længde), men samme effekt fås med en simpel guard i entiteten.
- **B: Id'er som typer (`ChatId`, `ParticipantId`, …).** Det beskytter mod at bytte om på to `Guid`s, men giver meget ceremoni og mapping i Dapper for lidt gevinst.
- **C: Medlemsperioden (`JoinedAt`, `LeftAt`).** Det er klassisk value object-stof (et tidsrum): slut kan ikke ligge før start, "aktiv" betyder "ingen slut", og en periode kan kun afsluttes én gang. Men reglerne findes først, når man kan forlade en chat.

## Beslutning
- Vi vælger **C** og bygger "forlad chat" som en ny slice, så value objectet har rigtige regler at håndhæve.
- **`MembershipPeriod`** (`DomainModel/ValueObjects`) er en `record` med `JoinedAt`, `LeftAt`, `IsActive` og `End(leftAt)`. Den er immutable: `End` returnerer en **ny**, afsluttet periode. Reglerne bor i typen:
  - `LeftAt` kan ikke ligge før `JoinedAt`.
  - En periode kan kun afsluttes én gang.
- `Participation` har en `Period` i stedet for to løse tidspunkter. `JoinedAt` og `LeftAt` findes stadig som read-only genveje, så Dapper kan finde `@JoinedAt` og `@LeftAt` i SQL'en.
- **Forlad chat** (`Chat.Leave(participantId)`) går gennem aggregate root'en, ligesom `AddParticipant` ([0010](0010-chat-styrer-deltagelse.md)):
  - **Man kan kun fjerne sig selv.** At smide andre ud kræver roller (admin/ejer), som vi ikke har.
  - Kun et **aktivt** medlem kan forlade chatten. Ellers `ForbiddenException` → `403` med samme besked som andre ikke-medlemmer ([0013](0013-adgang-til-chats-og-mindst-mulig-information.md)). Det gælder også, hvis chatten eller brugeren ikke findes.
  - Medlemskabet **slettes ikke**, det afsluttes (`left_at` sættes). Historikken bevares, og man kan tilføjes igen med en ny participation.
  - Chatten må gerne ende under 2 medlemmer, også på 0. Reglen "mindst 2" gælder kun ved oprettelse ([0012](0012-chat-oprettes-med-medlemmer.md)).
  - Loftet på 50 tæller kun **aktive** medlemmer.
- API: `DELETE /api/chats/{chatId}/participants/{userId}` → `204 No Content`, med sin egen route i Ocelot ([0001](0001-api-gateway-med-ocelot.md)). `DELETE`, fordi deltageren set fra klienten er væk, selvom rækken bliver liggende. `{userId}` i stien er midlertidig: med login bliver det `.../participants/me`.
- **Intet event** (`ParticipantLeft`) endnu. Ingen anden service har brug for det, og modtagerne af `MessageSent` får allerede kun aktive medlemmer ([0017](0017-modtagere-sendes-med-i-messagesent.md)).
- A og B fravælges for nu. Navn og indhold valideres fortsat i controller og contracts, og id'er er almindelige `Guid`s.

## Konsekvenser
- ✅ Domænemodellen har et value object med rigtige regler, ikke bare en indpakket primitiv.
- ✅ "Aktivt medlemskab" er defineret ét sted (`MembershipPeriod.IsActive`) i stedet for `LeftAt is null` spredt rundt i domænet.
- ✅ Handleren er tynd: opslag, ét kald til domænet, gem. Reglerne ligger i `Chat` og `MembershipPeriod`.
- ✅ Ingen ændring i databasen, læsninger eller `MessageSent`: dem, der har forladt chatten, mister automatisk adgang og modtager ikke nye beskeder.
- ⚠️ Repository'et skal oversætte mellem `MembershipPeriod` og to kolonner (`joined_at`, `left_at`). Det klares med genvejene på `Participation`.
- ⚠️ To samtidige "forlad"-requests kan begge læse medlemskabet som aktivt. Konsekvensen er kun, at `left_at` skrives to gange med næsten samme tid. Det accepterer vi, ligesom 49→51 i [0010](0010-chat-styrer-deltagelse.md).
- ⚠️ En chat kan ende uden aktive medlemmer. Den bliver liggende i databasen, men ingen kan se den.
- ⚠️ Kendt hul (som [0013](0013-adgang-til-chats-og-mindst-mulig-information.md)): uden login kan man "forlade" på vegne af en anden ved at sende deres user-id.
