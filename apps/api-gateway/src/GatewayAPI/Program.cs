using Ocelot.DependencyInjection;
using Ocelot.Middleware;

// ===== FASE 1: Konfiguration og services (hvad appen HAR) =====
// Alt med builder.Configuration... og builder.Services... skal stå FØR Build()

// 1. Laver en "builder", som samler konfiguration, logging og DI-containeren. Altid først.
var builder = WebApplication.CreateBuilder(args);

// 2. Indlæser Ocelot.json i konfigurationen (ASP.NET læser kun appsettings.json af sig selv).
//    optional: false  -> appen crasher ved opstart, hvis filen mangler (i stedet for stille 404 på alt)
//    reloadOnChange: true -> ændringer i routes opdages uden genstart
//    Skal stå FØR AddOcelot, fordi AddOcelot læser "Routes" fra konfigurationen.
builder.Configuration.AddJsonFile("Ocelot.json", optional: false, reloadOnChange: true);

// 3. Registrerer Ocelots egne klasser i DI (route-opslag, HTTP-kald videre til services osv.).
builder.Services.AddOcelot(builder.Configuration);

// 4. Bygger appen. Herefter er DI-containeren LÅST, og du kan ikke registrere flere services.
var app = builder.Build();

// ===== FASE 2: Pipeline (hvad der sker med hver request) =====
// Alt med app.Use... står EFTER Build() og køres i den rækkefølge, de står i

// 5. Ocelot tager imod requesten, finder en matchende route i Ocelot.json og sender den videre.
//    Ocelot er TERMINAL: den svarer selv (servicens svar eller 404), så intet efter den bliver nået.
//    Derfor står den SIDST i pipelinen. await, fordi UseOcelot returnerer en Task.
await app.UseOcelot();

// 6. Starter serveren. Altid sidst.
app.Run();
