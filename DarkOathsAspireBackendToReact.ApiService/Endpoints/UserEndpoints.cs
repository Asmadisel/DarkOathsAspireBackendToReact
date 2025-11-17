// ApiService/Endpoints/UsersEndpoints.cs
using DarkOathsAspireBackendToReact.ApiService.Data;
using DarkOathsAspireBackendToReact.ApiService.Models;
using Microsoft.EntityFrameworkCore;

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
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/users/{newUser.Id}", new { Id = newUser.Id, Login = newUser.Login, Email = newUser.Email });
        });
    }
}