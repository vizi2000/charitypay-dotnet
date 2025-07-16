using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CharityPay.Infrastructure.Data;
using CharityPay.Infrastructure.Data.Seed;

namespace CharityPay.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task<IHost> SeedDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<CharityPayDbContext>();
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
            
            var seeder = new DatabaseSeeder(context, logger);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database");
        }

        return host;
    }
}