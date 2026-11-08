# 0007: Contracts-projekt per service

Status: Accepteret (2026-11-06)

## Kontekst
En service' API består af request- og response-typer (DTO'er). Andre projekter (frontend, gateway, andre services) har brug for de samme typer. Ligger de inde i service-projektet, må andre projekter referere hele servicen – inklusive domænemodel og infrastruktur.

Alternativer:
- **DTO'er i service-projektet:** simpelt, men andre må enten referere hele servicen eller kopiere typerne.
- **Ét fælles DTO-projekt for alle services:** let at dele, men kobler alle services sammen.
- **Et contracts-projekt per service:** kun servicens offentlige API-typer.

## Beslutning
Hver service har et tilhørende `<Service>.Contracts`-projekt med dens offentlige DTO'er, f.eks. `ChatService.Contracts` med `CreateChatRequest` og `ChatDto`. Contracts-projektet må ikke referere servicens interne kode (entities, repositories).

## Konsekvenser
- ✅ Klar grænse mellem servicens offentlige API og dens interne model.
- ✅ Andre projekter kan bruge DTO'erne uden at afhænge af servicens implementering.
- ✅ Ændringer i domænemodellen bryder ikke klienter, så længe kontrakten er den samme.
- ⚠️ Flere projekter i solution.
- ⚠️ Mapping mellem entity og DTO skal skrives.
- ✅ Frontenden, RealTimeCommunicationService og ChatService bruger alle `ChatService.Contracts` (frontendens egen kopi af `CreateChatRequest` er fjernet).
- ⚠️ En project reference virker kun i vores eget repo. Skal en anden gruppes service bruge f.eks. `MessageSent`, skal kontrakten udgives som en NuGet-pakke eller beskrives som et JSON-schema. Det gør vi først, når nogen har brug for det.

## Hvad kontrakterne skjuler
`ChatService.Contracts` er ChatService' **shared model**: det, andre services og klienter skal kende for at integrere, og ikke mere. Kontrakterne er rene `record`s uden logik. De indeholder både requests/DTO'er (REST) og integration events (`Events/`, RabbitMQ).

| Internt i ChatService | Udadtil i kontrakterne | Hvorfor |
|---|---|---|
| `Participant` med eget id, forskelligt fra user-id ([0003](0003-userservice-er-source-of-truth.md)) | Kun `UserId` | Andre kender brugere, ikke ChatService' read model |
| `Participation` med historik og `MembershipPeriod` (`JoinedAt`, `LeftAt`) ([0019](0019-forlad-chat-og-medlemsperiode-som-value-object.md)) | `ChatMemberDto(UserId, Name)`: de aktive medlemmer lige nu | Ingen udenfor skal vide, hvem der var med før, eller hvordan medlemskab gemmes |
| Afsender er en participation (`message.sender_id`) | `SenderUserId` og `SenderName` | Samme grund: user-id er det fælles sprog |
| Regler: maks 50 medlemmer, kun ét aktivt medlemskab, kun medlemmer har adgang ([0010](0010-chat-styrer-deltagelse.md), [0013](0013-adgang-til-chats-og-mindst-mulig-information.md)) | Intet. Overtrædelser kommer som ProblemDetails ([0011](0011-fejlhaandtering-med-problemdetails.md)) | Reglerne kan ændres uden at ændre kontrakten |
| Hvem der er aktive medlemmer, når en besked sendes | `MessageSent.RecipientUserIds` ([0017](0017-modtagere-sendes-med-i-messagesent.md)) | Modtagerne skal ikke selv regne medlemskab ud |
| Tabeller, kolonner, indeks, Dapper-rækker (`ChatRow` m.fl.) | Intet | Databasen er privat for servicen ([0002](0002-database-per-service.md)) |

Testen på, om grænsen holder: da `Participation` fik `MembershipPeriod`, og man kunne forlade en chat ([0019](0019-forlad-chat-og-medlemsperiode-som-value-object.md)), blev **ingen** kontrakt ændret.
