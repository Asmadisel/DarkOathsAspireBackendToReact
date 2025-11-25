using DarkOathsAspireBackendToReact.AuthService.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DarkOathsAspireBackendToReact.AuthService.Endpoints
{
    public static class GoogleAuthEndpoints
    {
        // Эндпоинт для запуска процесса входа через Google
        public static void MapGoogleAuthEndpoints(this WebApplication app)
        {
            app.MapGet("/login-google", () =>
            {
                // Перенаправляем пользователя на Google для аутентификации
                return Results.Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = "/api/auth/google-callback",
                        Items = { { "prompt", "consent" } }
                    },
                    new[] { "Google" }
                );
            });

            // Эндпоинт-коллбэк, на который Google вернет пользователя
            // Эндпоинт-коллбэк, на который Google вернет пользователя
            app.MapGet("/api/auth/google-callback", async (
                HttpContext context,
                AuthDbContext dbContext
            ) =>
            {
                // 1. Явно аутентифицируем пользователя в схеме "GoogleCookies"
                var authenticateResult = await context.AuthenticateAsync("GoogleCookies");

                // 2. Проверяем, успешна ли аутентификация ВООБЩЕ
                if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
                {
                    Console.WriteLine($"Google auth failed: {authenticateResult.Failure?.Message}");
                    return Results.Unauthorized();
                }

                
                // 3. Извлекаем данные, используя правильные типы claims
                var claimsPrincipal = authenticateResult.Principal;

                // Способ 1: Использовать свойства Identity (имя уже есть!)
                var name = claimsPrincipal.Identity?.Name; // Это будет "Дмитрий Иванов"

                // Способ 2: Найти email вручную по его стандартному типу
                var email = claimsPrincipal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

                // ИЛИ (что эквивалентно) использовать константу:
                // var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

                // Защита от null
                name = name ?? "Unknown User";

                if (string.IsNullOrEmpty(email))
                {
                    // Добавьте отладочный вывод ВСЕХ claims, чтобы точно знать, что пришло
                    Console.WriteLine("=== ALL CLAIMS DUMP ===");
                    foreach (var claim in claimsPrincipal.Claims)
                    {
                        Console.WriteLine($"Type: '{claim.Type}', Value: '{claim.Value}'");
                    }
                    Console.WriteLine("========================");

                    Console.WriteLine("Google did not provide an email address.");
                    return Results.Unauthorized();
                }

                // 5. Теперь мы УВЕРЕНЫ, что email не null, и можем его использовать
                var localUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (localUser == null)
                {
                    var defaultRole = await dbContext.Roles
                     .FirstOrDefaultAsync(r => r.Name == "User");

                    if (defaultRole == null)
                    {
                        Console.WriteLine("FATAL ERROR: Role 'User' not found in database!");
                        return Results.StatusCode(500); // Internal Server Error
                    }

                    localUser = new AuthUser
                    {
                        Id = Guid.NewGuid(),
                        Email = email, // Безопасно, так как email не null
                        Login = name ?? email.Split('@')[0],
                        PasswordHash = "N/A",
                        Provider = "Google",
                        RoleId = defaultRole.Id,
                    };
                    dbContext.Users.Add(localUser);
                    await dbContext.SaveChangesAsync();
                }

                // 6. Генерация токена и редирект
                var jwtToken = GenerateJwtToken(localUser);
                await context.SignOutAsync("GoogleCookies"); // Очищаем временную сессию
                return Results.Redirect($"http://localhost:3000/auth-success?token={jwtToken}");
            });
        }
        private static string GenerateJwtToken(AuthUser user)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User") // Добавляем роль
            };

            var key = Encoding.ASCII.GetBytes("your_super_long_and_secure_secret_key_here_32_characters_min");
            var token = new JwtSecurityToken(
                issuer: "DarkOathsAuth",
                audience: "DarkOathsApp",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
