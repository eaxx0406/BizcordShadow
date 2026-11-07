# 0004: CQRS-light med MediatR

Status: Accepteret (2026-11-06)

## Kontekst
Uden struktur ender forretningslogik let i controllerne, som så bliver store og svære at teste. Læsning og skrivning har desuden forskellige behov: skrivning skal validere og håndhæve regler, læsning skal være hurtig og returnere præcis det, klienten har brug for.

Alternativer:
- **Logik i controllere:** hurtigt i starten, men svært at vedligeholde.
- **Service-klasser (f.eks. `ChatService.CreateChat()`):** klassisk lagdeling, men service-klasserne vokser sig store.
- **Commands/queries med handlers via MediatR:** én klasse per use case.

## Beslutning
- Hver use case er en **command** (skriv) eller **query** (læs) med en tilhørende **handler**, f.eks. `CreateChatCommand` + `CreateChatHandler`.
- Controllere er tynde: de oversætter HTTP-request → command, sender den via `IMediator` og mapper resultatet til en DTO.
- Repositories er delt i **write** og **read** (f.eks. `IChatWriteRepository`). Hver service ejer selv sine interfaces (se [0009](0009-ingen-delt-domaenekode-mellem-services.md)).
- Vi kalder det "CQRS-light", fordi læsning og skrivning bruger samme database – vi adskiller kun koden, ikke datalagringen.

## Konsekvenser
- ✅ Én use case = én handler; nemt at finde og teste isoleret.
- ✅ Tynde controllere.
- ✅ Klar adskillelse af læse- og skrivekode gør det muligt senere at optimere dem hver for sig.
- ⚠️ Flere filer per feature (command, handler, evt. DTO).
- ⚠️ Ekstra afhængighed af MediatR. Bemærk: nyere versioner af MediatR har ændret licens – tjek version før opgradering.
