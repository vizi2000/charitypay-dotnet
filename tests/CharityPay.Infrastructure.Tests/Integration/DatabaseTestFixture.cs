using CharityPay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace CharityPay.Infrastructure.Tests.Integration;

/// <summary>
/// Database test fixture using TestContainers for real PostgreSQL testing
/// </summary>
public class DatabaseTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private IServiceProvider _serviceProvider;

    public CharityPayDbContext DbContext { get; private set; }
    public string ConnectionString { get; private set; }

    public DatabaseTestFixture()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("charitypay_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start PostgreSQL container
        await _postgres.StartAsync();
        ConnectionString = _postgres.GetConnectionString();

        // Setup services
        var services = new ServiceCollection();
        
        // Add configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", ConnectionString)
            })
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);

        // Add DbContext with real PostgreSQL
        services.AddDbContext<CharityPayDbContext>(options =>
            options.UseNpgsql(ConnectionString));

        // Add logging
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        _serviceProvider = services.BuildServiceProvider();
        DbContext = _serviceProvider.GetRequiredService<CharityPayDbContext>();

        // Apply migrations
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }
        
        _serviceProvider?.Dispose();
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// Recreates the database with a clean schema
    /// </summary>
    public async Task RecreateDatabase()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.MigrateAsync();
    }

    /// <summary>
    /// Clears all data from the database but keeps the schema
    /// </summary>
    public async Task ClearAllData()
    {
        // Disable foreign key constraints temporarily
        await DbContext.Database.ExecuteSqlRawAsync("SET session_replication_role = replica;");

        try
        {
            // Get all table names
            var tableNames = await DbContext.Database
                .SqlQueryRaw<string>(@"
                    SELECT table_name 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_type = 'BASE TABLE'
                    AND table_name != '__EFMigrationsHistory'")
                .ToListAsync();

            // Truncate all tables
            foreach (var tableName in tableNames)
            {
                await DbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE;");
            }
        }
        finally
        {
            // Re-enable foreign key constraints
            await DbContext.Database.ExecuteSqlRawAsync("SET session_replication_role = DEFAULT;");
        }
    }
}

/// <summary>
/// Collection fixture for sharing DatabaseTestFixture across test classes
/// </summary>
[CollectionDefinition(nameof(DatabaseTestCollection))]
public class DatabaseTestCollection : ICollectionFixture<DatabaseTestFixture>
{
}

/// <summary>
/// Base class for database integration tests
/// </summary>
[Collection(nameof(DatabaseTestCollection))]
public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected readonly DatabaseTestFixture Fixture;
    protected readonly CharityPayDbContext DbContext;

    protected DatabaseTestBase(DatabaseTestFixture fixture)
    {
        Fixture = fixture;
        DbContext = fixture.DbContext;
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.ClearAllData();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes a function within a database transaction that is rolled back
    /// </summary>
    protected async Task<T> ExecuteInTransactionAsync<T>(Func<CharityPayDbContext, Task<T>> action)
    {
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            var result = await action(DbContext);
            await transaction.RollbackAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Executes an action within a database transaction that is rolled back
    /// </summary>
    protected async Task ExecuteInTransactionAsync(Func<CharityPayDbContext, Task> action)
    {
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            await action(DbContext);
            await transaction.RollbackAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Asserts that the database contains the expected number of records for an entity
    /// </summary>
    protected async Task AssertEntityCountAsync<T>(int expectedCount) where T : class
    {
        var actualCount = await DbContext.Set<T>().CountAsync();
        Assert.Equal(expectedCount, actualCount);
    }

    /// <summary>
    /// Asserts that an entity exists in the database with the given predicate
    /// </summary>
    protected async Task AssertEntityExistsAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
    {
        var exists = await DbContext.Set<T>().AnyAsync(predicate);
        Assert.True(exists, $"Expected entity of type {typeof(T).Name} to exist with the given predicate");
    }

    /// <summary>
    /// Asserts that no entity exists in the database with the given predicate
    /// </summary>
    protected async Task AssertEntityDoesNotExistAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
    {
        var exists = await DbContext.Set<T>().AnyAsync(predicate);
        Assert.False(exists, $"Expected no entity of type {typeof(T).Name} to exist with the given predicate");
    }
}

/// <summary>
/// Performance test fixture for measuring database operation performance
/// </summary>
public class DatabasePerformanceFixture : DatabaseTestFixture
{
    private readonly List<TimeSpan> _measurements = new();

    /// <summary>
    /// Measures the execution time of a database operation
    /// </summary>
    public async Task<T> MeasureAsync<T>(Func<CharityPayDbContext, Task<T>> operation, string operationName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await operation(DbContext);
        stopwatch.Stop();
        
        _measurements.Add(stopwatch.Elapsed);
        
        if (!string.IsNullOrEmpty(operationName))
        {
            Console.WriteLine($"{operationName}: {stopwatch.ElapsedMilliseconds}ms");
        }
        
        return result;
    }

    /// <summary>
    /// Gets performance statistics for all measured operations
    /// </summary>
    public (TimeSpan Min, TimeSpan Max, TimeSpan Average, int Count) GetPerformanceStats()
    {
        if (!_measurements.Any())
            return (TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, 0);

        return (
            Min: _measurements.Min(),
            Max: _measurements.Max(),
            Average: TimeSpan.FromTicks((long)_measurements.Average(t => t.Ticks)),
            Count: _measurements.Count
        );
    }

    /// <summary>
    /// Clears all performance measurements
    /// </summary>
    public void ClearMeasurements() => _measurements.Clear();
}

/// <summary>
/// Helper class for creating test data in the database
/// </summary>
public static class DatabaseTestDataHelper
{
    /// <summary>
    /// Seeds the database with a realistic test dataset
    /// </summary>
    public static async Task SeedRealisticDataAsync(CharityPayDbContext context)
    {
        // Create admin user
        var adminUser = CharityPay.Domain.Tests.Builders.TestDataBuilders.User()
            .AsAdmin()
            .WithEmail("admin@charitypay.com")
            .WithName("System", "Administrator")
            .Build();

        context.Users.Add(adminUser);

        // Create organization users and organizations
        var organizations = new List<CharityPay.Domain.Entities.Organization>();
        for (int i = 0; i < 10; i++)
        {
            var orgUser = CharityPay.Domain.Tests.Builders.TestDataBuilders.User()
                .AsOrganization()
                .WithEmail($"org{i}@example.com")
                .Build();

            context.Users.Add(orgUser);

            var organization = CharityPay.Domain.Tests.Builders.TestDataBuilders.Organization()
                .WithUser(orgUser)
                .WithStatus(i < 8 ? CharityPay.Domain.Enums.OrganizationStatus.Active : CharityPay.Domain.Enums.OrganizationStatus.Pending)
                .Build();

            organizations.Add(organization);
            context.Organizations.Add(organization);
        }

        await context.SaveChangesAsync();

        // Create payments for approved organizations
        var approvedOrgs = organizations.Where(o => o.Status == CharityPay.Domain.Enums.OrganizationStatus.Active).ToList();
        foreach (var org in approvedOrgs)
        {
            var payments = CharityPay.Domain.Tests.Builders.TestDataBuilders.Payment()
                .WithOrganization(org)
                .BuildMany(Random.Shared.Next(5, 20));

            context.Payments.AddRange(payments);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a large dataset for performance testing
    /// </summary>
    public static async Task SeedLargeDatasetAsync(CharityPayDbContext context, int organizationCount = 100, int paymentsPerOrg = 50)
    {
        // Create organizations in batches
        const int batchSize = 20;
        var random = new Random();

        for (int batch = 0; batch < organizationCount / batchSize; batch++)
        {
            var organizations = new List<CharityPay.Domain.Entities.Organization>();
            
            for (int i = 0; i < batchSize; i++)
            {
                var orgUser = CharityPay.Domain.Tests.Builders.TestDataBuilders.User()
                    .AsOrganization()
                    .Build();

                context.Users.Add(orgUser);

                var organization = CharityPay.Domain.Tests.Builders.TestDataBuilders.Organization()
                    .WithUser(orgUser)
                    .AsActive()()
                    .Build();

                organizations.Add(organization);
                context.Organizations.Add(organization);
            }

            await context.SaveChangesAsync();

            // Create payments for this batch
            foreach (var org in organizations)
            {
                var paymentCount = random.Next(paymentsPerOrg / 2, paymentsPerOrg);
                var payments = CharityPay.Domain.Tests.Builders.TestDataBuilders.Payment()
                    .WithOrganization(org)
                    .BuildMany(paymentCount);

                context.Payments.AddRange(payments);
            }

            await context.SaveChangesAsync();
            
            // Clear tracking to avoid memory issues
            context.ChangeTracker.Clear();
        }
    }
}