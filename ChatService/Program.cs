using ChatService.Repositories;
using ChatService.Repositories.Interfaces;

// ===== FASE 1: Registrér services (hvad appen HAR) =====
// Alt med builder.Services.Add... skal stå FØR Build()

// 1. Laver en "builder", som samler konfiguration, logging og DI-containeren. Altid først.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//MediatR scans the assembly for handlers, requests, and notifications and registers them with the DI container.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register the IChatWriteRepository implementation with the DI container
builder.Services.AddScoped<IChatWriteRepository, ChatWriteRepository>();

// Dapper: map snake_case-kolonner (chat_id) til PascalCase-properties (ChatId), når der LÆSES fra databasen (se ADR 0006).
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// 2. Fortæller DI, at appen bruger controllers.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// 3. Registrerer OpenAPI, som beskriver dine endpoints.
builder.Services.AddOpenApi();

// 4. Bygger appen. Herefter er DI-containeren LÅST, og du kan ikke registrere flere services.
var app = builder.Build();

// ===== FASE 2: Pipeline (hvad der sker med hver request) =====
// Alt med app.Use... og app.Map... står EFTER Build() og køres i den rækkefølge, de står i

// Configure the HTTP request pipeline.
// 5. Tjekker, om appen kører i development.
if (app.Environment.IsDevelopment())
{
    // 6. Viser OpenAPI-beskrivelsen, men kun i development.
    app.MapOpenApi();
}

// (Ingen UseHttpsRedirection: ChatService kaldes over http af gatewayen, og en redirect ville afbryde POST-kald.)

// 7. Tjekker, om brugeren har adgang. Skal stå før endpoints bliver mappet.
app.UseAuthorization();

// 8. Kobler URL'er til dine controller-metoder.
app.MapControllers();

// 9. Starter serveren. Altid sidst.
app.Run();
