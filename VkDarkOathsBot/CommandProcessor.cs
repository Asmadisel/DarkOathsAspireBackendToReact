// VkDarkOathsBot/CommandProcessor.cs
using DarkOathsAspireBackendToReact.AuthService.Data;
using VkDarkOathsBot.Handlers;
using VkDarkOathsBot.Models;
using VkDarkOathsBot.Services;


namespace VkDarkOathsBot;

public static class CommandProcessor
{
    // Словарь для маппинга алиасов команд на их обработчики
    private static readonly Dictionary<string, Func<VkBotDbContext, AuthDbContext, VkMessageService, long, long, bool, Task>> _commandMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "register", RegisterCommandHandler.HandleAsync },
            { "регистрация", RegisterCommandHandler.HandleAsync },
            { "maininfo", MainInfoCommandHandler.HandleAsync },
            { "инфо", MainInfoCommandHandler.HandleAsync },
            // Добавляйте сюда новые команды и их алиасы
        };

    public static async Task ProcessCommandAsync(
        VkBotDbContext dbContext,
        AuthDbContext authDbContext,
        VkMessageService messageService,
        string text,
        long fromId,
        long peerId)
    {
        bool isPrivate = (peerId == fromId);

        // Проверяем, начинается ли сообщение с "/"
        if (!text.StartsWith('/'))
        {
            // Если это не команда, игнорируем (или можно добавить логику для других сценариев)
            return;
        }

        string command = ExtractCommand(text);

        if (_commandMap.TryGetValue(command, out var handler))
        {
            // Команда найдена, вызываем обработчик
            await handler(dbContext, authDbContext, messageService, fromId, peerId, isPrivate);
        }
        else
        {
            // Команда неизвестна, но начинается с "/"
            if (isPrivate)
            {
                await messageService.SendTextMessageAsync(
                    peerId,
                    "Неизвестная команда. Используйте /help для списка доступных команд.");
            }
            // В чатах молча игнорируем
        }
    }

    private static string ExtractCommand(string fullText)
    {
        // Убираем начальный "/"
        if (!fullText.StartsWith('/'))
            return string.Empty;

        // Находим первый пробел или конец строки
        int spaceIndex = fullText.IndexOf(' ', 1); // Ищем пробел после первого символа
        if (spaceIndex == -1)
            return fullText.Substring(1); // "/command" -> "command"
        else
            return fullText.Substring(1, spaceIndex - 1); // "/command arg" -> "command"
    }
}