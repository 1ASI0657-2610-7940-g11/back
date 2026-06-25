using Microsoft.EntityFrameworkCore;

namespace FuelTrack.Api.Infrastructure.Data;

public static class DatabaseMigrator
{
    public static async Task ApplyMigrationsAsync(IServiceProvider services, ILogger logger)
    {
        const int attempts = 10;
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<FuelTrackDbContext>();
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied.");
                return;
            }
            catch (Exception ex) when (attempt < attempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(attempt * 3, 15));
                logger.LogWarning(
                    ex,
                    "Database migration attempt {Attempt}/{Attempts} failed. Retrying in {Delay}s.",
                    attempt,
                    attempts,
                    delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }

        await using var finalScope = services.CreateAsyncScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<FuelTrackDbContext>();
        await finalDb.Database.MigrateAsync();
    }
}
