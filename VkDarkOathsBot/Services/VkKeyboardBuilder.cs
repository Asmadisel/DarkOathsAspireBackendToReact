// VkDarkOathsBot/Services/VkKeyboardBuilder.cs
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VkDarkOathsBot.Models;

namespace VkDarkOathsBot.Services;

public static class VkKeyboardBuilder
{
    public static string BuildKeyboard(IEnumerable<VkButtonDefinition> buttons, ILogger? logger = null)
    {
        if (!buttons.Any())
            return "{}";

        var buttonRows = new[]
        {
            buttons.Select(btn =>
            {
                // Создаем РАЗНЫЕ структуры для разных типов кнопок
                object action;

                if (btn.Type == "open_link")
                {
                    // Для open_link разрешены ТОЛЬКО type, label, link
                    action = new
                    {
                        type = "open_link",
                        label = btn.Label,
                        link = btn.Url
                    };
                }
                else // text button
                {
                    // Для text разрешены type, label, payload
                    action = new
                    {
                        type = "text",
                        label = btn.Label,
                        payload = btn.Payload ?? "{}"
                    };
                }

                // Цвет разрешен ТОЛЬКО для text кнопок
                var color = btn.Type == "text" ? btn.Color : (string?)null;

                return new
                {
                    action,
                    color
                };
            }).ToArray()
        };

        var keyboard = new
        {
            one_time = false,
            buttons = buttonRows
        };

        var keyboardJson = JsonSerializer.Serialize(keyboard, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = false
        });

        logger?.LogDebug("Создана клавиатура: {KeyboardJson}", keyboardJson);
        return keyboardJson;
    }
}