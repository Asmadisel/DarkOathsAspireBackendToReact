using DarkOathsAspireBackendToReact.ApiService.Data;
using DarkOathsAspireBackendToReact.ApiService.Endpoints;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Интеграция с Aspire
builder.AddServiceDefaults();

// Настройка CORS для React-приложения
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Настройка подключения к БД (Aspire сам подставит строку подключения)
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>()
                             .GetConnectionString("apidb");
    options.UseNpgsql(connectionString);
});

// Настройка Swagger для разработки
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware для разработки
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.MapUsersEndpoints();
app.MapDefaultEndpoints();

app.Run();