// VkDarkOathsBot/Models/VkBotDbContext.cs
using Microsoft.EntityFrameworkCore;
using DarkOathsAspireBackendToReact.AuthService.Data;

namespace VkDarkOathsBot.Models;

public class VkBotDbContext : DbContext
{
    public VkBotDbContext(DbContextOptions<VkBotDbContext> options) : base(options) { }

    public DbSet<UserVk> VkUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<UserVk>(entity =>
        {
            entity.ToTable("VkUsers");
            entity.HasIndex(vk => vk.VkId).IsUnique();
        });
    }
}