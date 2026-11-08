# 0017: ChatService sender modtagerne med i MessageSent

Status: Foreslået (2026-11-08)

## Kontekst
Når en besked er gemt, publisher ChatService eventet `MessageSent` til RabbitMQ. Andre services, fx RealTimeCommunication eller Notification, skal pushe beskeden eller vise en notifikation med preview. De skal vide, **hvem** der skal have den: de aktive deltagere i chatten minus afsenderen.

Kun ChatService ved, hvem der er med i en chat (se [0010](0010-chat-styrer-deltagelse.md)).

Alternativer:
- **A: ChatService lægger modtagerne i eventet** (`RecipientUserIds`). Listen beregnes, når beskeden sendes.
- **B: Modtageren bygger sin egen kopi af medlemskaber** ud fra events som `ParticipantJoined` og `ParticipantLeft`.
- **C: Modtageren spørger ChatService** (REST), når den får eventet.

B lyder løst koblet, men events kan komme i **forkert rækkefølge** eller blive forsinket. Hvis `ParticipantLeft` kommer efter `MessageSent`, tror modtageren, at den udsmidte stadig er med, og sender beskeden til hende. Det er en sikkerhedsfejl: hun ser indholdet i et preview, selvom ChatService ville nægte hende adgang til chatten (se [0013](0013-adgang-til-chats-og-mindst-mulig-information.md)).

C kobler modtageren tæt til ChatService igen: den virker kun, når ChatService kører, og svaret kan allerede være forældet.

## Beslutning
- Vi vælger **A**. `MessageSent` har `RecipientUserIds`: user-id'erne på chattens aktive deltagere (`left_at is null`) minus afsenderen, hentet i samme handler, som gemmer beskeden.
- Det er **user-id'er**, ikke participation-id'er. Andre services kender brugere, ikke ChatService' interne medlemskaber.
- Modtagerne bruger listen, som den er. De må ikke selv regne ud, hvem der er med i chatten.
- Eventet har også `Content` (et "fedt" event), så et preview kan vises uden at spørge ChatService.

## Konsekvenser
- ✅ ChatService afgør adgang ét sted. Listen passer til netop denne besked, uanset hvornår eventet ankommer.
- ✅ Modtagerne virker, selvom ChatService er nede (beskeden ligger i køen).
- ⚠️ Eventet vokser med chattens størrelse. Det er acceptabelt med loftet på 50 deltagere ([0010](0010-chat-styrer-deltagelse.md)).
- ⚠️ Beskedens indhold og modtagerne ligger nu også hos andre services. De skal behandle det lige så fortroligt som ChatService.
- ⚠️ Listen hentes ikke i samme transaktion som beskeden gemmes. Bliver nogen smidt ud i præcis det millisekund, kan de nå at komme med. Det accepterer vi.
- ⚠️ `MessageSent` er en kontrakt (se [0007](0007-contracts-projekt-per-service.md)). Ændringer i den rammer alle subscribers.

---

### Til at huske det: teenagepigernes chat 💅
En gruppe teenagepiger har en chat. De smider en af pigerne ud, fordi de skal dele en hemmelighed, hun ikke må høre (eller bagtale hende lidt). Hun er ikke længere i *the inner circle*.

Med alternativ B kommer `ParticipantLeft` og `MessageSent` i forkert rækkefølge hos notifikations-servicen. Den tror, hun stadig er med, og viser hende hemmeligheden i **previewet**. Når hun åbner chatten, kan hun ikke se beskeden, for ChatService siger nej. Men så er det for sent: hun ved alt, hvad hun har brug for, og så er fanden løs i Laksegade.

Derfor: **den, der ejer medlemskabet, bestemmer modtagerne.**
