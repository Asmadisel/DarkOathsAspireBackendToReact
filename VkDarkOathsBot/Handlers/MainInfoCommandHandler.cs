// VkDarkOathsBot/Handlers/MainInfoCommandHandler.cs
using DarkOathsAspireBackendToReact.AuthService.Data;
using Microsoft.EntityFrameworkCore;
using VkDarkOathsBot.Models;
using VkDarkOathsBot.Services;

namespace VkDarkOathsBot.Handlers;

public static class MainInfoCommandHandler
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

        // Выполняем JOIN запрос через Include
        var binding = await dbContext.VkUsers
            .FirstOrDefaultAsync(b => b.VkId == fromId.ToString());

        if (binding?.LinkedAuthUserId  == null)
        {
            await messageService.SendTextMessageAsync(peerId,
                "Ваш аккаунт ВКонтакте не привязан или основной аккаунт не найден.");
            return;
        }
        var authUser = await authDbContext.Users
            .Include(u => u.Role) // Загружаем связанную роль
            .FirstOrDefaultAsync(u => u.Id == binding.LinkedAuthUserId);

        if (authUser == null)
        {
            await messageService.SendTextMessageAsync(peerId,
                "Привязанный основной аккаунт не найден в системе.");
            return;
        }

        var response = $@"
            Успешно подключен!
            Имя: {authUser.Login}
            Email: {authUser.Email}
            Роль: {authUser.Role?.Name ?? "N/A"}
            Статус: Активен
            VK ID: {fromId}
                    ".Trim();

        await messageService.SendTextMessageAsync(peerId, response);
    }
}