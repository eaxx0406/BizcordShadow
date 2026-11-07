# 0001: API Gateway med Ocelot

Status: Accepteret (2026-11-06)

## Kontekst
Bizcord består af flere microservices (foreløbig ChatService, senere bl.a. UserService). Hvis frontend kalder hver service direkte, skal den kende alle services' adresser og porte, og hver ændring i service-strukturen kræver ændringer i frontend. Tværgående ting som authentication, logging og rate limiting skulle også løses i hver service.

Alternativer:
- **Frontend kalder services direkte:** simpelt, men tæt kobling mellem frontend og service-struktur.
- **Egen gateway med controllers:** fuld kontrol, men vi skal selv skrive al videresendelse.
- **Ocelot:** færdigt .NET-bibliotek, hvor routing konfigureres i JSON.
- **YARP:** Microsofts reverse proxy, mere fleksibel men mere at sætte op.

## Beslutning
Vi bruger en API Gateway (`GatewayAPI`) bygget med **Ocelot**. Frontend kalder kun gatewayen. Routes defineres i `apps/api-gateway/src/GatewayAPI/Ocelot.json` (upstream-sti/metode → downstream-service).

## Konsekvenser
- ✅ Frontend kender kun én adresse. Services kan flyttes/splittes uden at frontend ændres.
- ✅ Ét sted at tilføje tværgående funktionalitet (f.eks. authentication) senere.
- ✅ Ingen kode for routing – kun konfiguration.
- ⚠️ Ekstra netværkshop og en ekstra proces, der skal køre under udvikling.
- ⚠️ Gatewayen er et single point of failure.
- ⚠️ Hver ny endpoint kræver en route i `Ocelot.json`; glemmer man den, svarer gatewayen 404.
