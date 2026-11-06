# 0005: Dapper og database first

Status: Accepteret (2026-11-06)

## Kontekst
ChatService skal gemme og læse data i PostgreSQL. Vi skal vælge, hvordan C#-koden taler med databasen, og om schemaet styres fra koden eller fra databasen.

Alternativer:
- **Entity Framework Core (code first):** schema genereres fra C#-klasser via migrations; meget SQL skrives for os.
- **Dapper (database first):** schemaet skrives i SQL; vi skriver selv SQL i repositories, og Dapper mapper resultater til objekter.
- **Ren ADO.NET/Npgsql:** fuld kontrol, men meget gentaget mapping-kode.

## Beslutning
Vi bruger **Dapper** med **Npgsql** og arbejder **database first**: schemaet defineres i `database/chatservice/01-schema.sql`, og repositories skriver SQL, der retter sig efter det.

## Konsekvenser
- ✅ Fuld kontrol over og indsigt i den SQL, der køres.
- ✅ Databasen designes efter databaseprincipper (constraints, indexes), ikke efter C#-klasser.
- ✅ Let og hurtigt; ingen skjult "magi".
- ⚠️ Schema og SQL i repositories skal holdes synkrone manuelt – stavefejl i tabel/kolonnenavne opdages først ved kørsel.
- ⚠️ Ingen automatiske migrations; schemaændringer kræver egne scripts (eller nulstilling af den lokale database).
- ⚠️ Entities og tabeller kan glide fra hinanden og skal holdes synkrone manuelt (det skete f.eks. med `MessageReceipt`, der havde et `Id`, mens tabellen bruger en sammensat nøgle).
- ⚠️ Dapper udfylder ikke navigation properties. Entities refererer derfor andre aggregates med id'er (f.eks. `ChatId`), ikke objekter.
