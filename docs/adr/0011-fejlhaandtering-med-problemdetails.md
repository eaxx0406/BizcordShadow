# 0011: Fejlhåndtering med domæne-exceptions og ProblemDetails

Status: Accepteret (2026-11-07) – endnu ikke implementeret

## Kontekst
I dag bliver alle fejl i ChatService til `500 Internal Server Error`: en chat, der ikke findes, en bruger, der allerede er medlem, og ugyldigt input. Klienten kan ikke se forskel på "du har bedt om noget forkert" og "serveren er gået ned". Under udvikling får vi kun "noget gik galt" og må lede i loggen.

Vi skal beslutte, hvordan fejl opdages, hvordan de bliver til HTTP-statuskoder, og hvilket format svaret har.

Alternativer:
- **try/catch i hver controller:** eksplicit, men den samme kode gentages i hvert endpoint, og det er let at glemme.
- **Result-pattern** (handlere returnerer `Result<T>` i stedet for at kaste): fejl er synlige i metodesignaturen, men det giver meget ekstra kode i hver handler og kræver et bibliotek eller en egen implementering.
- **Domæne-exceptions og én global exception handler:** handlere og entities kaster en exception med mening, og ét sted oversætter den til HTTP.

## Beslutning
- ChatService definerer egne exception-typer for fejl, der giver mening i domænet:
  - "findes ikke" → `404 Not Found`
  - konflikt med en domæneregel (allerede medlem, chatten er fuld) → `409 Conflict`
- Entities og handlere kaster disse exceptions med en kort, forståelig besked, f.eks. "Chat findes ikke".
- **Én global exception handler** (`IExceptionHandler`) oversætter exceptions til et **ProblemDetails**-svar (RFC 9457) med `status`, `title` og `detail`.
- **Ukendte exceptions** bliver til `500`. Tekniske detaljer (exception-besked, stack trace) vises kun i Development og aldrig i produktion.
- **Databasefejl oversættes i repository'et.** En unique violation fra Postgres (SQLSTATE `23505`) på `ux_participation_active` bliver til "allerede medlem". Handlere og controllere skal ikke kende Postgres-fejlkoder.
- **Input-validering** (f.eks. tomt navn, manglende id) sker med DataAnnotations på request-typerne i `ChatService.Contracts`. `[ApiController]` svarer så automatisk `400 Bad Request` med en `ValidationProblemDetails`. Vi skifter til FluentValidation, hvis reglerne bliver for komplekse til DataAnnotations.
- Entities beskytter stadig deres egne regler (guards). Input-validering giver pæne fejlbeskeder, og guards sikrer, at et ugyldigt objekt aldrig kan opstå.

| Situation | Status |
|---|---|
| Ugyldigt input | 400 |
| Chat eller bruger findes ikke | 404 |
| Allerede medlem / chatten er fuld | 409 |
| Alt andet | 500 |

## Konsekvenser
- ✅ Alle endpoints svarer med samme fejlformat, så frontend kun skal kunne læse ét.
- ✅ Handlere forbliver rene og indeholder ingen HTTP-kode.
- ✅ Under udvikling ser vi præcis, hvad der gik galt.
- ✅ Ocelot sender statuskoder og body uændret videre, så frontend får ProblemDetails gennem gatewayen.
- ⚠️ Exceptions er usynlige i metodesignaturen: man kan ikke se på en handler, hvilke fejl den kan give.
- ⚠️ Exceptions bruges til forventede fejl. Det er lidt dyrere end en returværdi, men det er ubetydeligt ved vores belastning.
- ⚠️ `ChatService.Contracts` får DataAnnotations-attributter. De er en del af .NET, så der kommer ingen ekstra pakke.
- ⚠️ Fejlbeskeder på dansk i API'et er et valg. Skal API'et bruges af andre, kan det blive nødvendigt med fejlkoder ved siden af teksten.
