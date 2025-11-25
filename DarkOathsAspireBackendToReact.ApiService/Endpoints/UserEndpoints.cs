// ApiService/Endpoints/UsersEndpoints.cs
using DarkOathsAspireBackendToReact.ApiService.Data;
using DarkOathsAspireBackendToReact.ApiService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace DarkOathsAspireBackendToReact.ApiService.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        // Эндпоинт для регистрации (создания пользователя)
        group.MapPost("/", async (CreateUserRequest request, ApplicationDbContext dbContext) =>
        {
            var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
            if (existingUser != null)
            {
                return Results.Conflict("User with this login already exists.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Login = request.Login,
                // TODO: В продакшене используйте хеширование (например, BCrypt)!
                PasswordHash = request.Password,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/users/{newUser.Id}", new { Id = newUser.Id, Login = newUser.Login, Email = newUser.Email });
        });

        // НОВЫЙ ЭНДПОИНТ: Получение списка пользователей (требует аутентификации)
        group.MapGet("/", async (
             [FromQuery] int? page,
             [FromQuery] int? pageSize,
             ApplicationDbContext dbContext,
             [FromKeyedServices("redis")] IConnectionMultiplexer redis,
             [FromHeader(Name = "Authorization")] string? authorization 
         ) =>
        {
            // --- ШАГ 1: Проверка аутентификации ---
            // Устанавливаем значения по умолчанию ПОСЛЕ проверки аутентификации
            var currentPage = page ?? 1;
            var currentPageSize = pageSize ?? 10;

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Json(
                     new { error = "Access to this resource is forbidden." },
                     statusCode: 403 // <-- Используем 403
                 );
            }

            var sessionId = authorization["Bearer ".Length..].Trim();
            var redisDb = redis.GetDatabase();
            var userIdFromSession = await redisDb.StringGetAsync($"session:{sessionId}");

            if (userIdFromSession.IsNullOrEmpty)
            {
                return Results.Json(
                    new { error = "Access to this resource is forbidden." },
                    statusCode: 403 // <-- Используем 403
                );
            }
            // --- Конец проверки аутентификации ---

            // --- ШАГ 2: Пагинация ---
            const int MaxPageSize = 50;
            currentPageSize = Math.Min(currentPageSize, MaxPageSize);
            var skip = (currentPage - 1) * currentPageSize;

            // Запрашиваем пользователей из БД
            var users = await dbContext.Users
                .OrderBy(u => u.Id)
                .Skip(skip)
                .Take(currentPageSize)
                .Select(u => new { u.Id, u.Login, u.Email, u.CreatedAt })
                .ToListAsync();

            var totalCount = await dbContext.Users.CountAsync();

            return Results.Ok(new
            {
                Users = users,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize)
            });
        });
    }
}