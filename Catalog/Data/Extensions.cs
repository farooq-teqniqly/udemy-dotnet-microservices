using Microsoft.EntityFrameworkCore;

namespace Catalog.Data
{
  internal static class Extensions
  {
    internal static async Task UseMigrationAsync(
      this WebApplication app,
      bool forceMigration = false,
      CancellationToken ct = default
    )
    {
      ArgumentNullException.ThrowIfNull(app);

      using (var scope = app.Services.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

        var logger = scope
          .ServiceProvider.GetRequiredService<ILoggerFactory>()
          .CreateLogger("Catalog.Migrations");

        try
        {
          await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
          await DataSeeder.SeedAsync(dbContext, forceMigration, ct).ConfigureAwait(false);

          logger.LogInformation(
            "Database migrated and seed completed (forceMigration: {Force}).",
            forceMigration
          );
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Database migration/seed failed.");

          throw;
        }
      }
    }
  }
}
