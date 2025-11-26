// VkDarkOathsBot/Handlers/RegisterCommandHandler.cs
using DarkOathsAspireBackendToReact.AuthService.Data;
using Microsoft.Extensions.Logging;
using VkDarkOathsBot.Models;
using VkDarkOathsBot.Services;
using static System.Net.WebRequestMethods;

namespace VkDarkOathsBot.Handlers;

public static class RegisterCommandHandler
{
    public static async Task HandleAsync(
        VkBotDbContext dbContext,
        AuthDbContext authDbContext,
        VkMessageService messageService,
        long fromId,
        long peerId,
        bool isPrivate)
    {
        if (!isPrivate)
        {
            await messageService.SendTextMessageAsync(peerId, "Эта команда работает только в личных сообщениях.");
            return;
        }

        var code = Guid.NewGuid().ToString("N");
        var site = "localhost:3000"; // Лучше вынести в конфиг
        var link = $"http://{site}/link-vk?code={code}";

        dbContext.VkUsers.Add(new UserVk { VkId = fromId.ToString(), LinkingCode = code });
        await dbContext.SaveChangesAsync();

        var buttons = new[]
         {
            new VkButtonDefinition
            {
                Label = "🔗 Привязать аккаунт",
                Url = "https://vk.com/asmadisaremellior",
                Color = "positive"
            },
            new VkButtonDefinition
            {
                Label = "❓ Помощь",
                Payload = "{\"action\":\"help\"}",
                Color = "secondary"
            }
        };


        await messageService.SendMessageWithButtonsAsync(
            peerId,
            $"Нажмите кнопку для привязки: {link}",
            buttons);
    }
}