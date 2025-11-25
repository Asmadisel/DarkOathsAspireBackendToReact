using DarkOathsAspireBackendToReact.AuthService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DarkOathsAspireBackendToReact.AuthService.Endpoints
{

    public static class UsersManagementEndpoints
    {
        public static void AddAdminAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));
            });
        }

        public static void MapUsersManagementEndpoints(this WebApplication app)
        {
            // Регистрируем политику в приложении
            var group = app.MapGroup("/api/users")
            .RequireAuthorization("RequireAdminRole");

            // GET: Получить всех пользователей (Только для Admin)
            group.MapGet("/", async (AuthDbContext dbContext) =>
            {
                // Используем async/await для асинхронного запроса
                var users = await dbContext.Users
                    .Include(u => u.Role)
                    .ToListAsync(); // <-- ToListAsync() для асинхронности
                return Results.Ok(new { users });
            });

            // POST: Добавить нового пользователя (Только для Admin)
            group.MapPost("/", async (AuthDbContext dbContext, NewUserRequest request) =>
            {
                var newUser = new AuthUser
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    Login = request.Login,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Provider = "Local"
                };

                var defaultRole = await dbContext.Roles // <-- await
                    .FirstOrDefaultAsync(r => r.Name == "User");
                if (defaultRole != null)
                    newUser.RoleId = defaultRole.Id;

                dbContext.Users.Add(newUser);
                await dbContext.SaveChangesAsync(); // <-- await
                return Results.Created($"/api/users/{newUser.Id}", newUser);
            });

            // PUT: Обновить пользователя (Только для Admin)
            group.MapPut("/{id:guid}", async (AuthDbContext dbContext, Guid id, UpdateUserRequest request) =>
            {
                var user = await dbContext.Users.FindAsync(id); // <-- FindAsync()
                if (user == null)
                    return Results.NotFound();

                user.Login = request.Login ?? user.Login;
                user.Email = request.Email ?? user.Email;
                if (!string.IsNullOrEmpty(request.Password))
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                await dbContext.SaveChangesAsync(); // <-- await
                return Results.Ok(user);
            });

            // DELETE: Удалить пользователя (Только для Admin)
            group.MapDelete("/{id:guid}", async (AuthDbContext dbContext, Guid id) =>
            {
                var user = await dbContext.Users.FindAsync(id); // <-- FindAsync()
                if (user == null)
                    return Results.NotFound();

                dbContext.Users.Remove(user);
                await dbContext.SaveChangesAsync(); // <-- await
                return Results.NoContent();
            });
        }
    }

    public record NewUserRequest(string Email, string Login, string Password);
    public record UpdateUserRequest(string? Login = null, string? Email = null, string? Password = null);
}
