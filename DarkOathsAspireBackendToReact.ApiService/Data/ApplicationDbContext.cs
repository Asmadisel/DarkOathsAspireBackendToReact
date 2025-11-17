using Microsoft.EntityFrameworkCore;
using DarkOathsAspireBackendToReact.ApiService.Models;

namespace DarkOathsAspireBackendToReact.ApiService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Заполняем таблицу Users начальными данными
            modelBuilder.Entity<User>().HasData(new User
            {
                Id =  new Guid("12345678-1234-1234-1234-123456789012"),
                Email = "admin@example.com",
                Login = "Admin",
                // НИКОГДА не храните пароли в открытом виде в коде или миграциях!
                // Для примера используем простой текст, но в реальном приложении это должен быть хэш.
                PasswordHash = "admin" // <-- Это временно! Замените на хэш в продакшене.
            });
        }
    }
}
