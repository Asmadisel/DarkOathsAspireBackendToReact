// VkDarkOathsBot/Handlers/PayloadHandler.cs
using DarkOathsAspireBackendToReact.AuthService.Data;
using System.Text.Json;
using VkDarkOathsBot.Models;
using VkDarkOathsBot.Services;

namespace VkDarkOathsBot.Handlers;

public static class PayloadHandler
{
    public static async Task HandlePayloadAsync(
        VkBotDbContext vkDbContext,
        AuthDbContext authDbContext,
        VkMessageService messageService,
        string payloadJson,
        long fromId,
        long peerId,
        bool isPrivate)
    {
        try
        {
            // Парсим payload как JSON
            using JsonDocument doc = JsonDocument.Parse(payloadJson);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("action", out JsonElement actionElement))
            {
                string action = actionElement.GetString() ?? "";

                switch (action)
                {
                    case "help":
                        await SendHelpAsync(messageService, peerId);
                        break;

                    case "cancel":
                        await messageService.SendTextMessageAsync(peerId, "Операция отменена.");
                        break;

                    default:
                        await messageService.SendTextMessageAsync(peerId, "Неизвестное действие.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            await messageService.SendTextMessageAsync(peerId, "Ошибка при обработке команды.");
        }
    }

    private static async Task SendHelpAsync(VkMessageService messageService, long peerId)
    {
        string helpText = @"
            Доступные команды:
            /register - привязать аккаунт
            /maininfo - информация об аккаунте

            Нажмите на соответствующие кнопки для быстрого доступа!";

        // Можно отправить новые кнопки
        var buttons = new[]
        {
            new VkButtonDefinition
            {
                Label = "🔗 Привязать аккаунт",
                Url = "https://your-domain.com/link-vk"
            },
            new VkButtonDefinition
            {
                Label = "ℹ️ Информация",
                Payload = "{\"action\":\"info\"}",
                Color = "primary"
            }
        };

        await messageService.SendMessageWithButtonsAsync(peerId, helpText, buttons);
    }
}