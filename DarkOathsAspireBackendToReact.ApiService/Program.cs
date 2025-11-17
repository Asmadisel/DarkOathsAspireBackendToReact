// ApiService/Program.cs
using DarkOathsAspireBackendToReact.ApiService;
using DarkOathsAspireBackendToReact.ApiService.Data;
using DarkOathsAspireBackendToReact.ApiService.Endpoints;
using DarkOathsAspireBackendToReact.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Адрес вашего React-приложения
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddKeyedSingleton<IConnectionMultiplexer>("redis", (sp, key) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString(key.ToString()!);
    return ConnectionMultiplexer.Connect(connectionString!);
});

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("postgresdb");
    options.UseNpgsql(connectionString);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// === КРИТИЧЕСКИ ВАЖНЫЙ БЛОК ===
// Применяем миграции СИНХРОННО и ОБЯЗАТЕЛЬНО до app.Run()
var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
dbContext.Database.Migrate(); 
// =============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.MapAuthEndpoints();
app.MapUsersEndpoints();

app.MapDefaultEndpoints();

app.Run();