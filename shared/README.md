# shared

Fælles **teknisk infrastruktur**, som flere services bruger. Ikke en app: intet her deployes alene.

Reglen er "del infrastruktur – aldrig domæne" ([ADR 0009](../docs/adr/0009-ingen-delt-domaenekode-mellem-services.md), [ADR 0018](../docs/adr/0018-delt-infrastruktur-i-shared.md)):

- ✅ Kode uden forretningsviden: message broker, logging, health checks.
- ❌ Domænekode (entities, regler) og kontrakter. Kontrakter ejes af den service, der udgiver dem (`<Service>.Contracts`, ADR 0007).

| Projekt | Rolle |
|---|---|
| `src/MessageClient` | Publish/subscribe via RabbitMQ (EasyNetQ). `builder.Services.AddMessageClient(connectionString)` + `IMessageClient`. |

Dockerfiler for services, der bruger noget herfra, skal kopiere `shared/src/` med (de bygges fra roden, ADR 0016).
