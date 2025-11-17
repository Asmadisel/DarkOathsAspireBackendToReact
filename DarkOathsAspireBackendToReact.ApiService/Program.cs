using DarkOathsAspireBackendToReact.ApiService.Data;
using DarkOathsAspireBackendToReact.ApiService.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.AddServiceDefaults();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Строка подключения 'postgres' не найдена.");
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

app.MapPost("/api/auth/login", async (LoginRequest request, ApplicationDbContext dbContext) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
    if (user == null)
    {
        // Возвращаем 401, если пользователь не найден (не раскрываем, что именно не так)
        return Results.Unauthorized();
    }

    if (user.PasswordHash != request.Password) 
    {
        return Results.Unauthorized();
    }

    // Если всё хорошо, возвращаем успешный ответ.
    // В реальности здесь нужно создать и вернуть JWT-токен.
    return Results.Ok(new { Message = "Login successful", UserId = user.Id });
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();