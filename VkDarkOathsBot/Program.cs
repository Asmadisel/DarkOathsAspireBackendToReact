// VkDarkOathsBot/Program.cs
using DarkOathsAspireBackendToReact.AuthService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VkBotFramework;
using VkDarkOathsBot;
using VkDarkOathsBot.Models;
using VkDarkOathsBot.Services;
using VkDarkOathsBot.Handlers;


var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddJsonConsole();

// === РЕЖИМ МИГРАЦИЙ ===
if (args.Contains("--migrate"))
{
    Console.WriteLine("🔧 Запуск режима миграций для VkBotDbContext...");

    builder.Services.AddDbContext<VkBotDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("authdb")));

    var migrationHost = builder.Build();
    using var migrationScope = migrationHost.Services.CreateScope();
    var migrationContext = migrationScope.ServiceProvider.GetRequiredService<VkBotDbContext>(); // Уникальное имя!

    // Применяем ВСЕ накатанные миграции
    await migrationContext.Database.MigrateAsync(); // Используем уникальное имя!

    Console.WriteLine("✅ Миграции для VkBotDbContext успешно применены.");
    return; // Завершаем работу после применения миграций
}

// === РЕЖИМ СЛУЖБЫ БОТА ===
Console.WriteLine("🚀 Запуск VK Bot Service...");

const string COMMUNITY_URL = "https://vk.com/darkoaths";

builder.Services.AddDbContext<VkBotDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("authdb")));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("authdb")));

builder.Services.AddSingleton<VkBot>(sp =>
{
    var accessToken = builder.Configuration["Vk:AccessToken"];
    if (string.IsNullOrWhiteSpace(accessToken))
    {
        throw new InvalidOperationException("VK Access Token is missing in configuration.");
    }
    return new VkBot(accessToken, COMMUNITY_URL);
});

builder.Services.AddScoped<VkMessageService>();

var botHost = builder.Build(); // Уникальное имя!

using var botScope = botHost.Services.CreateScope();
var botDbContext = botScope.ServiceProvider.GetRequiredService<VkBotDbContext>(); // Уникальное имя!
var authDbContext = botScope.ServiceProvider.GetRequiredService<AuthDbContext>();
var bot = botScope.ServiceProvider.GetRequiredService<VkBot>();
var messageService = botScope.ServiceProvider.GetRequiredService<VkMessageService>();

bot.OnMessageReceived += async (s, e) =>
{
    var msg = e.Message;
    if (!msg.FromId.HasValue || !msg.PeerId.HasValue) return;

    long fromId = msg.FromId.Value;
    long peerId = msg.PeerId.Value;
    bool isPrivate = (peerId == fromId);

    // Проверяем, есть ли payload (нажата кнопка)
    if (msg.Payload != null)
    {
        await PayloadHandler.HandlePayloadAsync(botDbContext, authDbContext, messageService, msg.Payload, fromId, peerId, isPrivate);
        return;
    }

    // Обычная обработка текстовых команд
    if (!string.IsNullOrEmpty(msg.Text))
    {
        await CommandProcessor.ProcessCommandAsync(botDbContext, authDbContext, messageService, msg.Text, fromId, peerId);
    }
};

bot.Start();
Console.WriteLine("✅ VK Bot Service успешно запущен и ожидает сообщения...");
await botHost.RunAsync(); 