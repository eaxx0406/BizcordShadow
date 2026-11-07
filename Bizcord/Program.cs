var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Typed HttpClient til GatewayAPI (frontend kalder kun gatewayen, ADR 0001)
builder.Services.AddHttpClient<Bizcord.Services.GatewayClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GatewayApi:BaseUrl"] ?? "http://localhost:5065/");
});

var app = builder.Build();

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
