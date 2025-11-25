namespace DarkOathsAspireBackendToReact.AuthService.Data
{
    public class AuthUser
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string Login { get; set; }
        public string? PasswordHash { get; set; } // Может быть null для Google
        public string Provider { get; set; } = "Local";
        public Guid RoleId { get; set; }
        public AuthRole Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
