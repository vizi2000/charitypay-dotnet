using AutoMapper;
using CharityPay.Application.Mappings;
using CharityPay.Application.Services;
using CharityPay.Application.Services.Interfaces;
using CharityPay.Domain.Tests.Builders;
using CharityPay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CharityPay.Application.Tests.Fixtures;

/// <summary>
/// Base test fixture for application layer tests with common dependencies
/// </summary>
public class ApplicationTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public ApplicationDbContext DbContext { get; }
    public IMapper Mapper { get; }

    public ApplicationTestFixture()
    {
        var services = new ServiceCollection();
        
        // Add in-memory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Add logging
        services.AddLogging(builder => builder.AddDebug());

        // Build service provider
        ServiceProvider = services.BuildServiceProvider();
        
        // Get instances
        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Mapper = ServiceProvider.GetRequiredService<IMapper>();

        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        // Create test users
        var adminUser = TestDataBuilders.User()
            .AsAdmin()
            .WithEmail("admin@charitypay.test")
            .Build();

        var orgUser = TestDataBuilders.User()
            .AsOrganization()
            .WithEmail("org@charitypay.test")
            .Build();

        DbContext.Users.AddRange(adminUser, orgUser);

        // Create test organizations
        var organizations = TestDataBuilders.Organization()
            .WithUser(orgUser)
            .BuildMany(3);

        DbContext.Organizations.AddRange(organizations);

        // Create test payments
        var payments = new List<CharityPay.Domain.Entities.Payment>();
        foreach (var org in organizations)
        {
            var orgPayments = TestDataBuilders.Payment()
                .WithOrganization(org)
                .BuildMany(5);
            payments.AddRange(orgPayments);
        }

        DbContext.Payments.AddRange(payments);

        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Clears all test data from the database
    /// </summary>
    public async Task ClearTestDataAsync()
    {
        DbContext.Payments.RemoveRange(DbContext.Payments);
        DbContext.Organizations.RemoveRange(DbContext.Organizations);
        DbContext.Users.RemoveRange(DbContext.Users);
        await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        ServiceProvider?.GetService<IServiceScope>()?.Dispose();
    }
}

/// <summary>
/// Collection fixture for sharing ApplicationTestFixture across test classes
/// </summary>
[CollectionDefinition(nameof(ApplicationTestCollection))]
public class ApplicationTestCollection : ICollectionFixture<ApplicationTestFixture>
{
}

/// <summary>
/// Base class for application service tests with common setup
/// </summary>
[Collection(nameof(ApplicationTestCollection))]
public abstract class ApplicationTestBase : IAsyncLifetime
{
    protected readonly ApplicationTestFixture Fixture;
    protected readonly ApplicationDbContext DbContext;
    protected readonly IMapper Mapper;

    protected ApplicationTestBase(ApplicationTestFixture fixture)
    {
        Fixture = fixture;
        DbContext = fixture.DbContext;
        Mapper = fixture.Mapper;
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.ClearTestDataAsync();
        await Fixture.SeedTestDataAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a mock logger for the specified type
    /// </summary>
    protected static Mock<ILogger<T>> CreateMockLogger<T>() => new();

    /// <summary>
    /// Asserts that an action throws a specific exception with message
    /// </summary>
    protected static async Task AssertThrowsAsync<TException>(
        Func<Task> action, 
        string expectedMessage = null)
        where TException : Exception
    {
        var exception = await Assert.ThrowsAsync<TException>(action);
        if (!string.IsNullOrEmpty(expectedMessage))
        {
            Assert.Contains(expectedMessage, exception.Message);
        }
    }
}

/// <summary>
/// Service test helper for creating service instances with mocked dependencies
/// </summary>
public class ServiceTestHelper
{
    /// <summary>
    /// Creates an OrganizationService with mocked dependencies for testing
    /// </summary>
    public static OrganizationService CreateOrganizationService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<OrganizationService> logger = null)
    {
        logger ??= Mock.Of<ILogger<OrganizationService>>();
        return new OrganizationService(context, mapper, logger);
    }

    /// <summary>
    /// Creates a PaymentService with mocked dependencies for testing
    /// </summary>
    public static PaymentService CreatePaymentService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<PaymentService> logger = null)
    {
        logger ??= Mock.Of<ILogger<PaymentService>>();
        return new PaymentService(context, mapper, logger);
    }

    /// <summary>
    /// Creates an AuthenticationService with mocked dependencies for testing
    /// </summary>
    public static AuthenticationService CreateAuthenticationService(
        ApplicationDbContext context,
        IPasswordService passwordService = null,
        IJwtService jwtService = null,
        ILogger<AuthenticationService> logger = null)
    {
        passwordService ??= Mock.Of<IPasswordService>();
        jwtService ??= Mock.Of<IJwtService>();
        logger ??= Mock.Of<ILogger<AuthenticationService>>();
        
        return new AuthenticationService(context, passwordService, jwtService, logger);
    }
}

/// <summary>
/// Custom attributes for test categorization
/// </summary>
public class UnitTestAttribute : FactAttribute
{
    public UnitTestAttribute()
    {
        Traits.Add("Category", "Unit");
    }
}

public class IntegrationTestAttribute : FactAttribute
{
    public IntegrationTestAttribute()
    {
        Traits.Add("Category", "Integration");
    }
}

public class SlowTestAttribute : FactAttribute
{
    public SlowTestAttribute()
    {
        Traits.Add("Speed", "Slow");
    }
}

/// <summary>
/// Test data scenarios for common testing patterns
/// </summary>
public static class TestScenarios
{
    public static readonly object[][] ValidEmailFormats = new[]
    {
        new object[] { "test@example.com" },
        new object[] { "user.name@domain.co.uk" },
        new object[] { "user+tag@example.org" },
        new object[] { "123@numbers.com" }
    };

    public static readonly object[][] InvalidEmailFormats = new[]
    {
        new object[] { "" },
        new object[] { "invalid" },
        new object[] { "@domain.com" },
        new object[] { "user@" },
        new object[] { "user@domain" }
    };

    public static readonly object[][] ValidPhoneNumbers = new[]
    {
        new object[] { "+48123456789" },
        new object[] { "123-456-789" },
        new object[] { "123 456 789" },
        new object[] { "48123456789" }
    };

    public static readonly object[][] PaymentAmounts = new[]
    {
        new object[] { 10.00m, true },   // Valid minimum
        new object[] { 500.00m, true },  // Valid typical
        new object[] { 10000.00m, true }, // Valid maximum
        new object[] { 5.00m, false },   // Too small
        new object[] { 0.00m, false },   // Zero
        new object[] { -10.00m, false }  // Negative
    };
}