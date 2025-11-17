// ApiService/MigrationBackgroundService.cs
using DarkOathsAspireBackendToReact.ApiService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DarkOathsAspireBackendToReact.ApiService;

public class MigrationBackgroundService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationBackgroundService> _logger;

    public MigrationBackgroundService(IServiceProvider serviceProvider, ILogger<MigrationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Применение миграций базы данных...");

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Небольшая задержка для уверенности, что PostgreSQL готов
            await Task.Delay(2000, cancellationToken);
            await context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Миграции успешно применены.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при применении миграций.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}