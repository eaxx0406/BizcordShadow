var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Typed HttpClient til GatewayAPI (frontend kalder kun gatewayen, ADR 0001)
builder.Services.AddHttpClient<Bizcord.Services.GatewayClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GatewayApi:BaseUrl"] ?? "http://localhost:5065/");
});

// "Dev login": den valgte testbruger læses fra en cookie (UserService er uden for scope)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Bizcord.Models.CurrentUser>();

var app = builder.Build();

// Skift testbruger fra dropdownen i topbaren. Gemmer brugerens id i cookien og går tilbage til siden.
app.MapGet("/switch-user/{id:guid}", (Guid id, string? returnUrl, HttpContext context) =>
{
    if (Bizcord.Models.TestUsers.CanLogIn.Any(u => u.Id == id))
    {
        context.Response.Cookies.Append(Bizcord.Models.CurrentUser.CookieName, id.ToString(),
            new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax, IsEssential = true });
    }

    // Kun lokale adresser, så linket ikke kan sende en videre til en anden side (open redirect)
    var isLocal = returnUrl is not null && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\");
    return Results.LocalRedirect(isLocal ? returnUrl! : "/");
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
