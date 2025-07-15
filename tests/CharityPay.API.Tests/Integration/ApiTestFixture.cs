using CharityPay.API;
using CharityPay.Application.DTOs.Auth;
using CharityPay.Domain.Tests.Builders;
using CharityPay.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Testcontainers.PostgreSql;
using Xunit;

namespace CharityPay.API.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for API integration testing
/// </summary>
public class CharityPayWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    public string ConnectionString { get; private set; }

    public CharityPayWebApplicationFactory()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("charitypay_test_api")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration for testing
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", ConnectionString),
                new KeyValuePair<string, string>("JwtSettings:SecretKey", "test-secret-key-that-is-long-enough-for-testing-purposes-12345"),
                new KeyValuePair<string, string>("JwtSettings:Issuer", "CharityPayTest"),
                new KeyValuePair<string, string>("JwtSettings:Audience", "CharityPayTestAudience"),
                new KeyValuePair<string, string>("JwtSettings:ExpirationMinutes", "60"),
                new KeyValuePair<string, string>("FiservSettings:BaseUrl", "https://api-sandbox.fiserv.com"),
                new KeyValuePair<string, string>("FiservSettings:ApiKey", "test-api-key"),
                new KeyValuePair<string, string>("FiservSettings:ApiSecret", "test-api-secret"),
                new KeyValuePair<string, string>("FiservSettings:StoreId", "test-store-id")
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CharityPayDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test database context
            services.AddDbContext<CharityPayDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            // Configure logging for tests
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        ConnectionString = _postgres.GetConnectionString();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// Gets a scoped service from the test server
    /// </summary>
    public T GetRequiredService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Creates a scope and returns the scoped service provider
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }
}

/// <summary>
/// API test fixture with authentication and database helpers
/// </summary>
public class ApiTestFixture : IAsyncLifetime
{
    private CharityPayWebApplicationFactory _factory;
    
    public HttpClient Client { get; private set; }
    public CharityPayWebApplicationFactory Factory => _factory;

    public async Task InitializeAsync()
    {
        _factory = new CharityPayWebApplicationFactory();
        await _factory.InitializeAsync();
        
        Client = _factory.CreateClient();
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Apply migrations and seed initial data
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CharityPayDbContext>();
        await context.Database.MigrateAsync();
        await SeedTestDataAsync(context);
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
    }

    /// <summary>
    /// Seeds the database with test data for API testing
    /// </summary>
    private async Task SeedTestDataAsync(CharityPayDbContext context)
    {
        // Create admin user
        var adminUser = TestDataBuilders.User()
            .AsAdmin()
            .WithEmail("admin@test.com")
            .WithName("Test", "Admin")
            .Build();

        // Create organization user
        var orgUser = TestDataBuilders.User()
            .AsOrganization()
            .WithEmail("org@test.com")
            .WithName("Test", "Organization")
            .Build();

        context.Users.AddRange(adminUser, orgUser);

        // Create organization
        var organization = TestDataBuilders.Organization()
            .WithUser(orgUser)
            .WithName("Test Organization")
            .AsApproved()
            .Build();

        context.Organizations.Add(organization);

        // Create some payments
        var payments = TestDataBuilders.Payment()
            .WithOrganization(organization)
            .BuildMany(5);

        context.Payments.AddRange(payments);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Authenticates as admin and returns the JWT token
    /// </summary>
    public async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Password123!"
        };

        var response = await PostAsync("/api/v1/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return loginResponse.Token;
    }

    /// <summary>
    /// Authenticates as organization and returns the JWT token
    /// </summary>
    public async Task<string> GetOrganizationTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "org@test.com",
            Password = "Password123!"
        };

        var response = await PostAsync("/api/v1/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return loginResponse.Token;
    }

    /// <summary>
    /// Sets the authorization header with the provided token
    /// </summary>
    public void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Removes the authorization header
    /// </summary>
    public void ClearAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Helper method for POST requests with JSON content
    /// </summary>
    public async Task<HttpResponseMessage> PostAsync<T>(string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PostAsync(requestUri, httpContent);
    }

    /// <summary>
    /// Helper method for PUT requests with JSON content
    /// </summary>
    public async Task<HttpResponseMessage> PutAsync<T>(string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PutAsync(requestUri, httpContent);
    }

    /// <summary>
    /// Helper method for PATCH requests with JSON content
    /// </summary>
    public async Task<HttpResponseMessage> PatchAsync<T>(string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PatchAsync(requestUri, httpContent);
    }

    /// <summary>
    /// Helper method to deserialize response content
    /// </summary>
    public async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CharityPayDbContext>();
        
        context.Payments.RemoveRange(context.Payments);
        context.Organizations.RemoveRange(context.Organizations);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the database context for direct database operations in tests
    /// </summary>
    public CharityPayDbContext GetDbContext()
    {
        var scope = _factory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<CharityPayDbContext>();
    }
}

/// <summary>
/// Collection fixture for sharing ApiTestFixture across test classes
/// </summary>
[CollectionDefinition(nameof(ApiTestCollection))]
public class ApiTestCollection : ICollectionFixture<ApiTestFixture>
{
}

/// <summary>
/// Base class for API integration tests
/// </summary>
[Collection(nameof(ApiTestCollection))]
public abstract class ApiTestBase : IAsyncLifetime
{
    protected readonly ApiTestFixture Fixture;
    protected readonly HttpClient Client;

    protected ApiTestBase(ApiTestFixture fixture)
    {
        Fixture = fixture;
        Client = fixture.Client;
    }

    public virtual Task InitializeAsync()
    {
        // Clear auth header before each test
        Fixture.ClearAuthorizationHeader();
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Authenticates the client with admin credentials
    /// </summary>
    protected async Task AuthenticateAsAdminAsync()
    {
        var token = await Fixture.GetAdminTokenAsync();
        Fixture.SetAuthorizationHeader(token);
    }

    /// <summary>
    /// Authenticates the client with organization credentials
    /// </summary>
    protected async Task AuthenticateAsOrganizationAsync()
    {
        var token = await Fixture.GetOrganizationTokenAsync();
        Fixture.SetAuthorizationHeader(token);
    }

    /// <summary>
    /// Asserts that the response has the expected status code
    /// </summary>
    protected static void AssertStatusCode(HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    /// <summary>
    /// Asserts that the response is successful (2xx status code)
    /// </summary>
    protected static void AssertSuccess(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, $"Expected success status code but got {response.StatusCode}. Content: {response.Content.ReadAsStringAsync().Result}");
    }

    /// <summary>
    /// Asserts that the response contains specific content
    /// </summary>
    protected static async Task AssertResponseContainsAsync(HttpResponseMessage response, string expectedContent)
    {
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedContent, content);
    }
}

/// <summary>
/// Helper class for creating test requests
/// </summary>
public static class TestRequestBuilder
{
    public static LoginRequest CreateLoginRequest(string email = "test@example.com", string password = "Password123!")
    {
        return new LoginRequest
        {
            Email = email,
            Password = password
        };
    }

    public static RegisterOrganizationRequest CreateRegisterRequest(
        string email = "org@example.com",
        string password = "Password123!",
        string organizationName = "Test Organization")
    {
        return new RegisterOrganizationRequest
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User",
            OrganizationName = organizationName,
            Description = "Test organization description",
            Category = CharityPay.Domain.Enums.OrganizationCategory.Religia,
            ContactEmail = email,
            ContactPhone = "+48123456789",
            Address = "Test Address 123",
            City = "Test City",
            PostalCode = "12-345",
            Country = "Poland",
            BankAccount = "PL61109010140000071219812874",
            TaxId = "123-456-78-90"
        };
    }
}