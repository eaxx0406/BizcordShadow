# 0002: Database per service

Status: Accepteret (2026-11-06)

## Kontekst
Microservices skal kunne udvikles, ændres og deployes uafhængigt. Deler flere services samme database, bliver de koblet gennem tabellerne: én service kan ikke ændre sit schema uden at risikere at ødelægge en anden.

Alternativer:
- **Fælles database for alle services:** nemt at joine på tværs, men tæt kobling.
- **Database per service:** hver service ejer sine data alene.

## Beslutning
Hver service har sin egen database, som kun den selv tilgår. ChatService ejer `chatdb`. Andre services får kun adgang til ChatService' data gennem dens API (eller events), aldrig direkte i databasen.

## Konsekvenser
- ✅ Services er løst koblede; schemaændringer påvirker kun ejeren.
- ✅ Hver service kan vælge den database, der passer bedst.
- ⚠️ Ingen joins på tværs af services. Data, der skal bruges på tværs, skal kopieres (se [0003](0003-userservice-er-source-of-truth.md)).
- ⚠️ Ingen transaktioner på tværs af services – konsistens på tværs bliver *eventual*.
- ⚠️ Flere databaser at drive og starte lokalt.
