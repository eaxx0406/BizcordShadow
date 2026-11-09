-- Testdata indtil UserService findes og kan levere participants (read model) via events.
-- Matcher de hardcodede brugere i apps/web/src/Bizcord/Models/TestUsers.cs.
-- id (participant) er med vilje FORSKELLIG fra user_id, så det kan ses, at oversættelsen user-id -> participant-id virker.
-- "Ukendt bruger" (9999...) er med vilje IKKE med, så fejlen "bruger findes ikke" kan testes.
-- Tidspunkter er relative til now(), så chatsene altid ser friske ud, når databasen oprettes.
-- Køres KUN, når databasen oprettes (tom volume). Se README.md i roden for, hvordan man nulstiller.

-- 1) Brugere (participants). user_id = 10000000-...-0000000000NN, id = aaaaaaaa-...-0000000000NN
insert into participant (id, user_id, name)
select ('aaaaaaaa-0000-0000-0000-' || lpad(nr::text, 12, '0'))::uuid,
       ('10000000-0000-0000-0000-' || lpad(nr::text, 12, '0'))::uuid,
       name
from (values
    -- De sædvanlige + Sommerhus + Roadtrip
    (1, 'Emma'), (2, 'Oliver'), (3, 'Noah'), (4, 'Sofie'), (5, 'Lucas'),
    (6, 'Freja'), (7, 'Mikkel'), (8, 'Clara'), (9, 'Anton'),
    -- Girlies
    (10, 'Alma'), (11, 'Sofia'), (12, 'Mille'), (13, 'Laura'),
    -- Gamere + band
    (14, 'Victor'), (15, 'Liam'), (16, 'Oscar'), (17, 'Felix'), (33, 'Malthe'), (34, 'Asger'),
    -- Familien
    (18, 'Mor'), (19, 'Far'), (20, 'Ida'), (21, 'Emil'), (22, 'Mormor'),
    -- Studiegruppen
    (23, 'Sander'), (24, 'Peter'), (25, 'Ea'), (26, 'Bjørn'),
    -- Kontoret
    (27, 'Henrik'), (28, 'Bente'), (29, 'Kasper'), (30, 'Maja'), (31, 'Rasmus'), (32, 'Pia')
) as u(nr, name);

-- 2) Chats. id = c0000000-...-0000000000NN (nr bruges nedenfor)
insert into chat (id, name)
select ('c0000000-0000-0000-0000-' || lpad(nr::text, 12, '0'))::uuid, name
from (values
    (1,  '🍕 De sædvanlige'),
    (2,  '🎉 Operation Surprise'),
    (3,  '💅 Girlies 💗'),
    (4,  '🐍 Girlies (uden Emma)'),
    (5,  '🏕️ Sommerhus 2027 ☀️'),
    (6,  '🎮 Ranked & Raging'),
    (7,  '👨‍👩‍👧‍👦 Familien Kaos'),
    (8,  '🖥️ Det virker på min maskine'),
    (9,  '💘 Bare os to'),
    (10, '🕵️ Hvem tog min mad?!'),
    (11, '🎸 Garagebandet'),
    (12, '🎢 Roadtrip & Rutsjebaner')
) as c(nr, name);

-- 3) Medlemskaber. left_minutes = for hvor mange minutter siden de forlod chatten (null = stadig med)
insert into participation (id, chat_id, participant_id, joined_at, left_at)
select gen_random_uuid(),
       ('c0000000-0000-0000-0000-' || lpad(m.chat::text, 12, '0'))::uuid,
       p.id,
       now() - interval '60 days',
       now() - make_interval(mins => m.left_minutes)
from (values
    (1, 'Emma', null::int), (1, 'Oliver', null), (1, 'Noah', null), (1, 'Sofie', null), (1, 'Lucas', null),
    (2, 'Freja', null), (2, 'Mikkel', null), (2, 'Clara', null), (2, 'Anton', null),
    (3, 'Alma', null), (3, 'Sofia', null), (3, 'Mille', null), (3, 'Laura', null), (3, 'Emma', null),
    (4, 'Alma', null), (4, 'Sofia', null), (4, 'Mille', null), (4, 'Laura', null),
    (5, 'Emma', null), (5, 'Oliver', null), (5, 'Noah', null), (5, 'Sofie', null),
    (5, 'Lucas', null), (5, 'Freja', null), (5, 'Mikkel', null), (5, 'Clara', null),
    (6, 'Victor', null), (6, 'Liam', null), (6, 'Oscar', null), (6, 'Felix', null),
    (7, 'Mor', null), (7, 'Far', null), (7, 'Ida', null), (7, 'Emil', null), (7, 'Mormor', null),
    (8, 'Sander', null), (8, 'Peter', null), (8, 'Ea', null), (8, 'Bjørn', null),
    (9, 'Oliver', null), (9, 'Emma', null),
    -- Kasper har forladt chatten (ADR 0019): hans gamle beskeder er der stadig, men han kan ikke se chatten
    (10, 'Henrik', null), (10, 'Bente', null), (10, 'Kasper', 6890), (10, 'Maja', null), (10, 'Rasmus', null), (10, 'Pia', null),
    (11, 'Malthe', null), (11, 'Asger', null), (11, 'Felix', null), (11, 'Lucas', null),
    (12, 'Anton', null), (12, 'Clara', null), (12, 'Noah', null), (12, 'Sofie', null), (12, 'Mikkel', null)
) as m(chat, name, left_minutes)
join participant p on p.name = m.name;

-- 4) Beskeder. minutes_ago = hvor mange minutter siden beskeden blev sendt (1440 = et døgn).
--    Afsenderen er en participation (medlemskab), så vi slår den op ud fra chat + navn.
insert into message (id, chat_id, sender_id, content, sent_at)
select gen_random_uuid(), pa.chat_id, pa.id, m.content, now() - make_interval(mins => m.minutes_ago)
from (values
    -- 🍕 De sædvanlige
    (1, 'Lucas',  2900, 'Filmaften fredag? Jeg har popcorn og absolut ingen planer 🍿'),
    (1, 'Sofie',  2880, 'JA. Men ikke gyser igen, jeg sov ikke i tre dage sidst'),
    (1, 'Noah',   2870, 'Gyser er den eneste rigtige genre'),
    (1, 'Emma',   2860, 'Romcom. Det er ikke til diskussion 💅'),
    (1, 'Oliver', 2855, 'Jeg er enig med Emma'),
    (1, 'Noah',   2850, 'Selvfølgelig er du det 👀'),
    (1, 'Lucas',  1500, 'Nå. Pizza. Hvem bestiller?'),
    (1, 'Sofie',  1495, 'Hawaii 🍍'),
    (1, 'Noah',   1494, 'Ananas på pizza er en forbrydelse'),
    (1, 'Emma',   1490, 'Pepperoni og ingen diskussion'),
    (1, 'Lucas',  1485, 'Vi har nu 4 forslag fra 5 personer. Oliver?'),
    (1, 'Oliver', 1480, 'Jeg tager bare det samme som Emma'),
    (1, 'Sofie',  1478, 'Hvorfor siger du ALTID det samme som Emma 🤔'),
    (1, 'Oliver', 1475, 'Det gør jeg da ikke'),
    (1, 'Noah',     45, 'Nogen der har set min oplader? Tror jeg glemte den hos dig Lucas'),
    (1, 'Lucas',    40, 'Den er min nu. Sådan er reglerne 😎'),

    -- 🎉 Operation Surprise (Noah må IKKE se den her)
    (2, 'Freja',  10000, 'OKAY. Noah fylder 20 om tre uger, og han må IKKE opdage noget 🤫'),
    (2, 'Mikkel',  9990, 'Jeg er med! Hvor holder vi det?'),
    (2, 'Clara',   9980, 'Mine forældre er i sommerhus den weekend, vi kan låne huset'),
    (2, 'Anton',   9970, 'Jeg kan stå for musikken. Og fyrværkeri?'),
    (2, 'Freja',   9965, 'Nej til fyrværkeri Anton'),
    (2, 'Anton',   9960, 'Lille smule fyrværkeri?'),
    (2, 'Freja',   9955, 'NEJ'),
    (2, 'Mikkel',  4000, 'Hvordan får vi ham derhen, uden han fatter mistanke?'),
    (2, 'Clara',   3990, 'Vi siger, det er brætspilsaften. Han elsker Settlers'),
    (2, 'Anton',   3980, 'Ingen elsker Settlers'),
    (2, 'Mikkel',   300, 'ALARM. Han spurgte lige, hvorfor vi alle sammen har travlt den lørdag 😬'),
    (2, 'Freja',    295, 'Hvad sagde du??'),
    (2, 'Mikkel',   290, 'At jeg skulle til tandlæge. En lørdag. Klokken 20.'),
    (2, 'Clara',    285, 'Mikkel 💀'),

    -- 💅 Girlies 💗
    (3, 'Mille', 5000, 'Hvem skal med til fest på lørdag?? 🎉'),
    (3, 'Laura', 4990, 'MIG. Men jeg har intet at tage på'),
    (3, 'Alma',  4985, 'Du har bogstaveligt talt 40 kjoler'),
    (3, 'Laura', 4980, 'Og alle er blevet set på Instagram 😩'),
    (3, 'Emma',  4970, 'Jeg kan ikke på lørdag, jeg har planer 🙈'),
    (3, 'Sofia', 4965, 'Planer med HVEM 👀'),
    (3, 'Emma',  4960, 'Bare... planer'),
    (3, 'Mille', 4955, 'Mistænkeligt.'),
    (3, 'Alma',  2000, 'Eksamen i fysik på mandag, og jeg har forstået præcis 0 %'),
    (3, 'Sofia', 1990, 'Vi kan læse sammen i morgen? Jeg tager kage med 🍰'),
    (3, 'Emma',  1985, 'Jeg er med!'),
    (3, 'Laura',  120, 'Har I set den nye frisure, Mads har fået 💀'),
    (3, 'Mille',  115, 'Han ligner en champignon 🍄'),
    (3, 'Emma',   110, 'Haha stop, han er sød nok 😂'),

    -- 🐍 Girlies (uden Emma)
    (4, 'Alma',  4900, 'Okay, men HVEM er det, Emma har planer med?'),
    (4, 'Mille', 4895, 'Hun har været mærkelig hele måneden. Altid på telefonen, og så smiler hun'),
    (4, 'Laura', 4890, 'Det er 100 % en fyr'),
    (4, 'Alma',  4885, 'Og hun har ikke engang fortalt OS det? Wow.'),
    (4, 'Mille', 4880, 'Vi er åbenbart ikke gode nok til at vide det 🙄'),
    (4, 'Sofia', 4870, 'Måske vil hun bare ikke sige det endnu?'),
    (4, 'Laura', 2500, 'Jeg så hende ved Netto med en fra hendes vennegruppe. Oliver, tror jeg'),
    (4, 'Alma',  2495, 'OLIVER?? Ham der altid er enig med hende?'),
    (4, 'Mille', 2490, '😭😭😭'),
    (4, 'Sofia', 2400, 'Synes I ikke, det er lidt unfair, at vi snakker om hende her?'),
    (4, 'Laura', 2395, 'Det er jo ikke ondt ment'),
    (4, 'Sofia',  100, 'Seriøst, jeg får dårlig samvittighed. Kan vi ikke bare spørge hende?'),
    (4, 'Alma',    95, 'Sofia nej. Så ved hun, at vi har en chat uden hende'),
    (4, 'Sofia',   90, 'Måske skulle jeg bare forlade den her chat så...'),

    -- 🏕️ Sommerhus 2027 ☀️
    (5, 'Freja',  20000, 'Sommerhus uge 28? Jeg har fundet et i Blokhus med spa 🛁'),
    (5, 'Lucas',  19990, 'Ja!! Hvad koster det?'),
    (5, 'Freja',  19985, '1.450 kr. per person. MobilePay til mig senest fredag 🙏'),
    (5, 'Clara',  19900, 'Overført ✅'),
    (5, 'Mikkel', 19800, 'Overført ✅'),
    (5, 'Emma',   19700, 'Overført ✅'),
    (5, 'Freja',  12000, 'Status: 4 ud af 8 har betalt. Jeg kigger på jer 👀'),
    (5, 'Noah',   11990, 'Jeg betaler, når jeg får SU'),
    (5, 'Lucas',  11980, 'Samme'),
    (5, 'Freja',  11975, 'I fik begge SU i sidste uge'),
    (5, 'Sofie',   6000, 'Hvem står for indkøb?'),
    (5, 'Noah',    5990, '...'),
    (5, 'Lucas',   5985, '...'),
    (5, 'Oliver',  5980, 'Emma og jeg kan godt tage det'),
    (5, 'Sofie',   5975, 'Emma OG dig? Igen? 🤨'),
    (5, 'Mikkel',   600, 'Vigtigt spørgsmål: må man tage sin egen hovedpude med, eller er det for meget'),
    (5, 'Clara',    590, 'Det er ALDRIG for meget'),

    -- 🎮 Ranked & Raging
    (6, 'Victor', 3000, 'Ranked i aften kl. 20. Ingen undskyldninger. Vi skal op i Diamond.'),
    (6, 'Liam',   2995, '[GIF: hund der nikker meget seriøst]'),
    (6, 'Oscar',  2990, 'Jeg kan først 20:30, skal spise med min mor'),
    (6, 'Victor', 2985, 'Din mor kan vente. Diamond kan ikke.'),
    (6, 'Felix',  2980, 'Bro, det er et spil'),
    (6, 'Victor', 2975, 'Det er IKKE bare et spil, Felix'),
    (6, 'Liam',   2970, '[GIF: mand der græder i regnen]'),
    (6, 'Victor', 1300, 'Hvem af jer gik AFK midt i sidste runde'),
    (6, 'Oscar',  1295, 'Min mor kaldte til aftensmad'),
    (6, 'Felix',  1290, 'Det var mig, jeg skulle tisse'),
    (6, 'Victor', 1285, 'I to er en skam for holdet.'),
    (6, 'Liam',   1280, '[GIF: brændende skraldespand]'),
    (6, 'Felix',    30, 'Pause fra ranked? Bare noget hyggeligt co-op?'),
    (6, 'Victor',   25, 'Hygge er for tabere'),
    (6, 'Liam',     20, '[GIF: kat der råber ad en skærm]'),

    -- 👨‍👩‍👧‍👦 Familien Kaos (Mormors godmorgenhilsner kommer i trin 5)
    (7, 'Mor',    2000, 'Hvem kommer til aftensmad i dag? Svar venligst 🙏'),
    (7, 'Far',    1990, 'Mig'),
    (7, 'Mor',    1900, 'Ida? Emil?'),
    (7, 'Mor',    1800, 'Hallo??'),
    (7, 'Emil',   1700, 'kommer måske'),
    (7, 'Mor',    1695, '"Måske" er ikke et svar, Emil'),
    (7, 'Ida',    1600, 'Jeg spiser hos en veninde ❤️'),
    (7, 'Mormor', 1500, 'Jeg kommer gerne! Skal jeg tage frikadeller med? 😘'),
    (7, 'Mor',    1495, 'Ja tak, mor ❤️'),
    (7, 'Far',    1490, 'Så kommer Emil helt sikkert'),
    (7, 'Emil',   1485, 'kommer'),
    (7, 'Mor',     200, 'Hvem har taget det sidste rugbrød? Der er kun endestykker tilbage'),
    (7, 'Ida',     195, 'Emil'),
    (7, 'Emil',    190, 'Far'),
    (7, 'Far',     185, 'Jeg nægter alt'),

    -- 🖥️ Det virker på min maskine (studiegruppen midt i projektet)
    -- Bjørn er senior-udvikler og hængte på, fordi han synes, projektet er spændende.
    -- Han opdager langsomt, at han er blevet gruppens uofficielle projektleder.
    -- Dag 1
    (8, 'Bjørn',  4479, 'Hej! Nå, hvad er I i gang med?'),
    (8, 'Ea',     4478, 'En chatservice med RabbitMQ og database. Vi er midt i event-delen.'),
    (8, 'Peter',  4477, 'Men først skal du lige se mit nye Proxmox-setup.'),
    (8, 'Sander', 4476, 'Har du fået VLANs op at køre?'),
    (8, 'Bjørn',  4475, 'Spændende! Men burde I ikke tage event-delen først?'),
    (8, 'Peter',  4474, 'Jo jo. Men Bjørn, se lige den her video 😂'),
    (8, 'Sander', 4340, 'Jeg har lige skrevet til læreren og spurgt, om vi må bruge Redis i stedet for SQL 📧'),
    (8, 'Peter',  4339, 'Jeg har fundet en video af en mand, der har bygget et Kubernetes-cluster af 12 Raspberry Pis.'),
    (8, 'Ea',     4338, 'Jeg har lavet en knap! Den virker næsten.'),
    (8, 'Bjørn',  4337, 'Hvor langt er I med selve opgaven?'),
    (8, 'Sander', 4336, 'Vi har arkitekturen på plads.'),
    (8, 'Bjørn',  4335, 'Har I noget, der kører?'),
    (8, 'Peter',  4334, 'Mit hjemmecluster kører.'),
    (8, 'Bjørn',  4333, '...'),
    (8, 'Ea',     4130, 'Sååå... API''et virker, frontenden virker, og vi har events i RabbitMQ! 🎉'),
    (8, 'Sander', 4129, 'Jeg har lavet integrationstests og dokumentation.'),
    (8, 'Peter',  4128, 'Og jeg har sat pipeline op. Skal hente børn nu 🏃'),
    (8, 'Bjørn',  4127, 'Godt arbejde. Så fik I jo lavet noget alligevel.'),
    (8, 'Peter',  4126, 'Tak Bjørn. Samme tid næste uge?'),
    (8, 'Bjørn',  4125, 'Skal jeg være med næste uge også?'),
    (8, 'Sander', 4124, 'Ja! Næste uge er det distributed tracing!'),
    (8, 'Peter',  4123, 'Jeg har allerede bestilt tre nye servere.'),
    (8, 'Bjørn',  4122, 'Selvfølgelig har du det.'),
    -- I dag
    (8, 'Sander',  300, 'Sidder i toget, men er på om 10 minutter 🚆'),
    (8, 'Peter',   299, 'Har lige opgraderet mit hjemmecluster. Nu har jeg Kubernetes, Grafana, Prometheus og 14 containere til at vise en side med teksten "Hello World".'),
    (8, 'Sander',  298, 'Nice. Hvilken hypervisor kører du?'),
    (8, 'Bjørn',   297, 'Drenge. Opgaven. 😭'),
    (8, 'Peter',   210, 'Kan vi ikke bare gemme det i en JSON-fil?'),
    (8, 'Sander',  209, 'Eller Redis.'),
    (8, 'Peter',   208, 'Eller slet ikke gemme noget. Så slipper vi også for GDPR.'),
    (8, 'Bjørn',   207, 'Der står SQL-database i opgaven.'),
    (8, 'Sander',  206, 'Jeg skriver lige til læreren og spørger, om det virkelig er nødvendigt 📧'),
    (8, 'Peter',   205, 'Hvorfor slog databasen op med sin kæreste? Fordi de ikke havde nogen relationer 🥁'),
    (8, 'Ea',      204, 'Jeg går ud til hesten 🐴'),
    (8, 'Bjørn',   203, 'Jeg meldte mig ind for at snakke arkitektur. Nu er jeg åbenbart projektleder 🫠'),
    (8, 'Peter',   120, '[YouTube: "Top 10 programmeringsjokes" – 12 min] Nr. 7 er SÅ god'),
    (8, 'Bjørn',   119, 'Peter. Fokus.'),
    (8, 'Peter',   118, 'Hvorfor bruger programmører mørkt tema? Fordi lys tiltrækker bugs 🥁'),
    (8, 'Bjørn',   117, 'Det var ikke svaret på "fokus".'),
    (8, 'Peter',   116, 'Okay okay, jeg kigger på consumeren nu'),
    (8, 'Peter',    90, '[YouTube: "Hvorfor Rust er fremtiden" – 47 min] bare til baggrundslyd'),
    (8, 'Peter',    60, '[YouTube: "Cat fixes production server" – 0:34] HAHAHAHA se lige den her'),
    (8, 'Sander',   58, 'Jeg har fået databasen op at køre i Docker og skrevet tests. Det hele er pushet.'),
    (8, 'Ea',       57, 'Fedt! Jeg har lavet "forlad chat" og et value object. Skal vi lige gennemgå koden sammen?'),
    (8, 'Peter',    56, 'Jeg skal hente børn. Men jeg har lavet PR og sat CI/CD op. Ses!'),
    (8, 'Sander',   55, 'Jeg er lige ved at miste forbindelsen. Toget kører ind i en tunnel.'),
    (8, 'Ea',       54, '...'),
    (8, 'Ea',       53, 'Så gennemgår jeg det bare med Claude 🙃'),
    (8, 'Bjørn',    52, 'Jeg er på Teams om 5 min. Del din skærm, så tager vi den sammen 🖥️'),
    (8, 'Sander',   20, 'Ude af tunnelen. Det virker på min maskine 🤷'),
    (8, 'Peter',    15, 'Så shipper vi din maskine 🐳'),

    -- 💘 Bare os to
    (9, 'Oliver', 3000, 'Godnat 🌙 Det var en rigtig hyggelig aften'),
    (9, 'Emma',   2995, 'Godnat 🥰 Tak for i dag'),
    (9, 'Emma',   1470, 'Sofie spurgte, hvorfor du altid er enig med mig 😂'),
    (9, 'Oliver', 1465, 'Fordi du altid har ret?'),
    (9, 'Emma',   1460, 'Godt svar 😌'),
    (9, 'Oliver', 1000, 'Skal vi ikke snart fortælle de andre det?'),
    (9, 'Emma',    995, 'Snart. Jeg tror, Sofie har gættet det'),
    (9, 'Oliver',  990, 'Noah kiggede også mærkeligt på mig til filmaften'),
    (9, 'Emma',     50, 'Pizza og film hos mig på lørdag? Bare os to 🍕'),
    (9, 'Oliver',   45, 'Pepperoni og ingen diskussion ❤️'),

    -- 🕵️ Hvem tog min mad?!
    (10, 'Bente',  7000, 'Hvem har taget min lasagne fra køleskabet?! Der stod BENTE på låget.'),
    (10, 'Henrik', 6990, 'Ikke mig. Jeg spiser kun rugbrød.'),
    (10, 'Maja',   6985, 'Det er tredje gang i denne måned 😠'),
    (10, 'Rasmus', 6980, 'Jeg starter en efterforskning 🕵️ Alle mistænkte bedes blive i nærheden'),
    (10, 'Pia',    6975, 'Kasper havde lasagne i går. Bare siger det.'),
    -- (Kasper skriver noget her, som han sletter igen – se trin 4b)
    (10, 'Pia',    6971, 'Kasper, hvad slettede du lige der? 👀'),
    (10, 'Kasper', 6970, 'Det var MIN lasagne! Den var fra Netto!'),
    (10, 'Rasmus', 6965, 'Interessant. Hvilken Netto?'),
    (10, 'Kasper', 6960, 'Det kan jeg ikke huske'),
    (10, 'Pia',    6950, 'Mistænkeligt.'),
    (10, 'Kasper', 6900, 'Jeg gider ikke det her mere.'),
    (10, 'Maja',   6880, 'Kasper forlod lige chatten 👀'),
    (10, 'Rasmus', 6875, 'En skyldig mand flygter. Sagen er lukket.'),
    (10, 'Bente',   300, 'Nyt offer: min yoghurt er væk. Kasper har ikke været her i dag.'),
    (10, 'Maja',    295, 'Så var det ikke ham...'),
    (10, 'Rasmus',  290, 'Sagen er genåbnet 🔍'),
    (10, 'Henrik',  280, '...jeg troede, den var min. Undskyld, Bente 😬'),

    -- 🎸 Garagebandet
    (11, 'Malthe', 9000, 'Forslag til bandnavn #14: "Støj i Garagen"'),
    (11, 'Asger',  8990, '#15: "De Ustemte"'),
    (11, 'Felix',  8985, '#16: "Lucas'' Mors Garage"'),
    (11, 'Lucas',  8980, 'Hun vil gerne have sin bil tilbage derind'),
    (11, 'Malthe', 5000, 'Øver vi torsdag?'),
    (11, 'Asger',  4990, 'Kan ikke, har håndbold'),
    (11, 'Felix',  4985, 'Min bas har kun 3 strenge lige nu'),
    (11, 'Lucas',  4980, '#17: "Tre Strenge"'),
    (11, 'Malthe', 4975, '...faktisk ret godt'),
    (11, 'Asger',   200, 'Vi har fået en koncert!! Til min fætters konfirmation 🎸'),
    (11, 'Felix',   195, 'Hvornår?'),
    (11, 'Asger',   190, 'Om 2 uger'),
    (11, 'Lucas',   185, 'Vi kan ikke en eneste sang'),
    (11, 'Malthe',  180, 'Så øver vi torsdag. INGEN undskyldninger.'),
    (11, 'Asger',   175, 'Jeg har håndbold'),

    -- 🎢 Roadtrip & Rutsjebaner
    (12, 'Anton',  8000, 'Roadtrip til en forlystelsespark i august!! Hvem er med? 🎢'),
    (12, 'Sofie',  7990, 'Mig!'),
    (12, 'Noah',   7985, 'Hvem kører?'),
    (12, 'Mikkel', 7980, 'Jeg har kørekort, men min bil har ikke aircondition'),
    (12, 'Clara',  7975, 'Jeg tager med, men jeg prøver INGEN rutsjebaner'),
    (12, 'Anton',  7970, 'Du SKAL prøve den store, Clara'),
    (12, 'Clara',  7965, 'Jeg skal ingenting'),
    (12, 'Anton',  3000, 'Jeg har lavet en plan: 14 forlystelser på 6 timer ⏱️'),
    (12, 'Sofie',  2990, 'Hvornår spiser vi?'),
    (12, 'Anton',  2985, 'I køen'),
    (12, 'Clara',  2980, 'Jeg holder jeres tasker og kigger på 😌'),
    (12, 'Mikkel',  600, 'Bilen er klar. Den lugter lidt af hund, men den kører 🐶'),
    (12, 'Anton',   590, 'PERFEKT'),
    (12, 'Clara',   585, 'Jeg har allerede købt køresygepiller')
) as m(chat, name, minutes_ago, content)
join participant p on p.name = m.name
join participation pa on pa.chat_id = ('c0000000-0000-0000-0000-' || lpad(m.chat::text, 12, '0'))::uuid
                     and pa.participant_id = p.id;

-- 4b) En slettet besked (soft delete, ADR 0020): rækken findes, men indholdet er tomt, og deleted_at er sat.
--     Kasper nåede at skrive noget om lasagnen, som han hurtigt slettede igen.
insert into message (id, chat_id, sender_id, content, sent_at, deleted_at)
select gen_random_uuid(), pa.chat_id, pa.id, '', now() - make_interval(mins => 6973), now() - make_interval(mins => 6972)
from participant p
join participation pa on pa.participant_id = p.id and pa.chat_id = 'c0000000-0000-0000-0000-000000000010'
where p.name = 'Kasper';

-- 4c) Systembeskeder (ADR 0022): content er tom, frontenden laver teksten ud fra system_type og details.
--     Rasmus omdøber chatten, da efterforskningen starter, og Kasper forlader chatten (samme tidspunkt som left_at i trin 3).
insert into message (id, chat_id, sender_id, content, sent_at, kind, system_type, details)
select gen_random_uuid(), pa.chat_id, pa.id, '', now() - make_interval(mins => s.minutes_ago), 'system', s.system_type, s.details::jsonb
from (values
    ('Rasmus', 6979, 'chat_renamed', '{"oldName": "🧊 Køkkenet, 2. sal", "newName": "🕵️ Hvem tog min mad?!"}'),
    ('Kasper', 6890, 'member_left',  null)
) as s(name, minutes_ago, system_type, details)
join participant p on p.name = s.name
join participation pa on pa.participant_id = p.id and pa.chat_id = 'c0000000-0000-0000-0000-000000000010';

-- 5) Mormor sender godmorgen hver dag i 35 dage, så der er nok beskeder til "Vis ældre beskeder" (keyset paging, ADR 0014)
insert into message (id, chat_id, sender_id, content, sent_at)
select gen_random_uuid(), pa.chat_id, pa.id,
       (array['Godmorgen alle sammen ☀️🌷',
              'Godmorgen! Husk at drikke vand 💧',
              'God morgen, mine skatter ❤️☕',
              'Godmorgen! Det bliver en dejlig dag i dag 🌞🌻',
              'GODMORGEN 🌄 (Emil, har du husket madpakken?)',
              'Godmorgen fra mormor 🐦🌸'])[1 + day % 6],
       date_trunc('day', now()) - make_interval(days => day) + interval '6 hours 42 minutes'
from generate_series(1, 35) as day
join participant p on p.name = 'Mormor'
join participation pa on pa.chat_id = 'c0000000-0000-0000-0000-000000000007' and pa.participant_id = p.id;
