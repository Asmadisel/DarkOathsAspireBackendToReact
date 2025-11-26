// VkDarkOathsBot/Services/VkMessageService.cs
using Microsoft.Extensions.Logging;
using VkBotFramework;
using VkDarkOathsBot.Models;

namespace VkDarkOathsBot.Services;

public class VkMessageService
{
    private readonly VkBot _bot;
    private readonly ILogger<VkMessageService> _logger;

    public VkMessageService(VkBot bot, ILogger<VkMessageService> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    public async Task SendMessageWithButtonsAsync(long userId, string message, IEnumerable<VkButtonDefinition> buttons)
    {
        try
        {
            // Используем отдельный конструктор для создания клавиатуры
            var keyboardJson = VkKeyboardBuilder.BuildKeyboard(buttons, _logger);

            await _bot.Api.CallAsync("messages.send", new VkNet.Utils.VkParameters
            {
                { "user_id", userId },
                { "message", message },
                { "keyboard", keyboardJson },
                { "random_id", Environment.TickCount },
                { "v", "5.236" }
            });

            _logger.LogInformation("Сообщение с клавиатурой отправлено пользователю {UserId}", userId);
        }
        catch (VkNet.Exception.VkApiException ex)
        {
            _logger.LogError(ex, "Ошибка VK API: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    public async Task SendTextMessageAsync(long peerId, string message)
    {
        await _bot.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams
        {
            PeerId = peerId,
            Message = message,
            RandomId = Environment.TickCount
        });
    }
}