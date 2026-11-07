# 0006: Postgres-navngivning med snake_case

Status: Accepteret (2026-11-06)

## Kontekst
PostgreSQL laver alle navne uden anførselstegn om til små bogstaver. Navne i anførselstegn (`"Chat"`, `"ChatId"`) er case-sensitive og skal derfor skrives med anførselstegn *alle* steder – i DDL, i SQL i repositories og i manuelle forespørgsler. Den stil kommer typisk fra Entity Framework/SQL Server.

Alternativer:
- **PascalCase i anførselstegn** (`"Chat"."ChatId"`): matcher C#-navnene, men anførselstegn overalt og let at lave fejl.
- **snake_case uden anførselstegn** (`chat.chat_id`): idiomatisk Postgres.

## Beslutning
Tabeller og kolonner navngives med **små bogstaver og snake_case uden anførselstegn**. Tabelnavne er i ental (`chat`, `participation`, `message_receipt`).

## Konsekvenser
- ✅ Idiomatisk Postgres – samme stil som i dokumentation og tidligere projekter.
- ✅ SQL kan skrives uden anførselstegn.
- ⚠️ Navne i C# (`ChatId`) og databasen (`chat_id`) er forskellige. Ved læsning skal Dapper mappe dem, f.eks. med `DefaultTypeMap.MatchNamesWithUnderscores = true` eller aliaser i SQL (`chat_id AS ChatId`).
