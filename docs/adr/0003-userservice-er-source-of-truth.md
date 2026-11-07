# 0003: UserService er source of truth – Participant er en read model

Status: Accepteret (2026-11-06) – synkronisering er endnu ikke implementeret

## Kontekst
ChatService skal vide, hvem der deltager i en chat (navn m.m.). Brugere ejes af UserService. Med database per service ([0002](0002-database-per-service.md)) kan ChatService ikke joine på UserService' tabeller.

Alternativer:
- **ChatService kalder UserService' API ved hvert opslag:** altid friske data, men ChatService virker ikke, når UserService er nede, og hvert opslag koster et netværkskald.
- **ChatService gemmer kun `UserId` og frontend slår navne op:** simpelt, men flytter problemet til frontend.
- **ChatService har sin egen kopi (read model) af de brugerdata, den har brug for:** hurtige opslag og uafhængighed, men data skal synkroniseres.

## Beslutning
- **UserService er source of truth** for brugere. Kun UserService må oprette og ændre brugere.
- ChatService har tabellen `participant` som en **read model** af en User: kun de felter, ChatService har brug for (`user_id`, `name`).
- Der er et **1:1-forhold** mellem User og Participant, sikret med `unique` på `participant.user_id`.
- Read modellen skal holdes opdateret via **events** fra UserService (f.eks. `UserCreated`, `UserRenamed`) over en message broker.

## Konsekvenser
- ✅ ChatService kan arbejde uden at kalde UserService, også når den er nede.
- ✅ Klart ejerskab: der er ét sted, brugerdata ændres.
- ⚠️ **Eventual consistency:** en ny eller omdøbt bruger når først frem til ChatService, når eventet er behandlet. Vi skal tage stilling til, hvad der sker, hvis en bruger opretter en chat, før dennes participant findes.
- ⚠️ Kræver en message broker og event-håndtering, som endnu ikke er bygget.
- ⚠️ `CreateChatHandler` skal slå participant op ud fra `UserId` i stedet for at bruge user-id direkte som participant-id.

## Midlertidig løsning
Indtil UserService og events findes, seedes en participant for testbrugeren i `database/chatservice/02-seed.sql`. Den har `id = user_id`, så den nuværende handler virker.

## Åbent spørgsmål
Når forholdet er 1:1, kunne `participant.id` være lig `user_id`, så den ekstra nøgle og opslaget falder væk. Det er et almindeligt mønster for read models og bør overvejes i en senere ADR.
