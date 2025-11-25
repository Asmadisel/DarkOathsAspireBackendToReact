using Microsoft.EntityFrameworkCore;

namespace DarkOathsAspireBackendToReact.AuthService.Data
{
    public class AuthDbContext : DbContext
    {

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AuthUser> Users { get; set; } = null!;
        public DbSet<AuthRole> Roles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<AuthUser>()
           .HasOne(u => u.Role)
           .WithMany()
           .HasForeignKey(u => u.RoleId);

            var userRole = new AuthRole { Id = RolesConstants.User, Name = "User" };
            var adminRole = new AuthRole { Id = RolesConstants.Admin, Name = "Admin" };
            modelBuilder.Entity<AuthRole>().HasData(userRole, adminRole);

            // === ДОБАВЛЕНИЕ АДМИН-ПОЛЬЗОВАТЕЛЯ ===
            modelBuilder.Entity<AuthUser>().HasData(new AuthUser
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"), 
                Email = "admin@darkoaths.local",
                Login = "admin",
                PasswordHash = "$2a$11$vMeiSGKL77ZlzUE66avB4eZNk2pzt47FWghlINFz8eIxui.g/S1Iu",
                Provider = "Local",
                RoleId = adminRole.Id,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) 
            });

            // ===================================
        }
    }
}
