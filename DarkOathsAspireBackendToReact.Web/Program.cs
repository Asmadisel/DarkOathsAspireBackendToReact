using DarkOathsAspireBackendToReact.Web.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// ✅ ДОБАВИТЬ Antiforgery сервисы (это обязательно)
builder.Services.AddAntiforgery();

// CORS для React dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// ✅ Добавить только контроллеры (Blazor не нужен для React)
builder.Services.AddControllers();

// HttpClient для сервисов
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new("https+http://authservice");
});

builder.Services.AddHttpClient("ApiService", client =>
{
    client.BaseAddress = new("https+http://apiservice");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ✅ ВАЖНО: Порядок middleware критичен!
app.UseDefaultFiles(); // Для index.html
app.UseStaticFiles();  // Для wwwroot

// ✅ Теперь UseAntiforgery будет работать
app.UseAntiforgery();

if (app.Environment.IsDevelopment())
{
    app.UseCors("ReactDev");
}

app.MapControllers();
app.MapDefaultEndpoints();

// ✅ Fallback на React index.html
app.MapFallbackToFile("index.html");

app.Run();