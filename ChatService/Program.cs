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

// 7. Sender http-requests videre til https.
app.UseHttpsRedirection();

// 8. Tjekker, om brugeren har adgang. Skal stå før endpoints bliver mappet.
app.UseAuthorization();

// 9. Kobler URL'er til dine controller-metoder.
app.MapControllers();

// 10. Starter serveren. Altid sidst.
app.Run();
