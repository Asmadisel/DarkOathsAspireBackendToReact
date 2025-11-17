using DarkOathsAspireBackendToReact.ApiService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.AddServiceDefaults();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Строка подключения 'postgresdb' не найдена.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 1. Добавляем генератор OpenAPI-документов (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate(); // <-- Эта строка и делает "update-database"
}

// 2. Подключаем middleware для отображения документации
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();                 // Служит JSON-документ
    app.UseSwaggerUI();               // Служит UI-интерфейс (Swagger UI)
}

// Ваши endpoint'ы
app.MapGet("/weatherforecast", () => "Hello World!")
   .WithName("GetWeatherForecast");

// Endpoint для Aspire
app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();