// ApiService/Endpoints/AuthEndpoints.cs
using DarkOathsAspireBackendToReact.ApiService.Data;
using DarkOathsAspireBackendToReact.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace DarkOathsAspireBackendToReact.ApiService.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        // Эндпоинт для входа (логин)
        group.MapPost("/login", async (
            LoginRequest request,
            ApplicationDbContext dbContext,
            [FromKeyedServices("redis")] IConnectionMultiplexer redis
        ) =>
        {
            var db = redis.GetDatabase();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
            if (user == null || user.PasswordHash != request.Password)
            {
                return Results.Unauthorized();
            }

            var sessionId = Guid.NewGuid().ToString();
            await db.StringSetAsync($"session:{sessionId}", user.Id.ToString(), TimeSpan.FromMinutes(30));
            return Results.Ok(new { SessionId = sessionId, Message = "Login successful" });
        });
    }
}