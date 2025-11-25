using DarkOathsAspireBackendToReact.AuthService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DarkOathsAspireBackendToReact.AuthService.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth");

            // Эндпоинт для логина
            group.MapPost("/login", async (LoginRequest req, AuthDbContext db) =>
            {
                var user = await db.Users
                 .Include(u => u.Role) // Явно загружаем связанную роль
                 .FirstOrDefaultAsync(u => u.Login == req.Login); ;

                if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                    return Results.Unauthorized();

                var token = GenerateJwtToken(user);
                return Results.Ok(new { Token = token });
            });

            // Эндпоинт для регистрации
            group.MapPost("/register", async (RegisterRequest req, AuthDbContext db) =>
            {
                if (await db.Users.AnyAsync(u => u.Login == req.Login))
                    return Results.Conflict(new { error = "User already exists." });

                var newUser = new AuthUser
                {
                    Id = Guid.NewGuid(),
                    Login = req.Login,
                    Email = req.Email,
                    RoleId = RolesConstants.User,
                    // TODO: Сохраняйте ХЕШ пароля, а не сам пароль!
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                    CreatedAt = DateTime.UtcNow
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                return Results.Created();
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

    public record LoginRequest(string Login, string Password);
    public record RegisterRequest(string Login, string Email, string Password);
}

