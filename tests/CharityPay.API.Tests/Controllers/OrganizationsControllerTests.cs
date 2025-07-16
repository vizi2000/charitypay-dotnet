using CharityPay.API.Tests.Integration;
using CharityPay.Application.DTOs.Organization;
using CharityPay.Domain.Enums;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace CharityPay.API.Tests.Controllers;

public class OrganizationsControllerTests : ApiTestBase
{
    public OrganizationsControllerTests(ApiTestFixture fixture) : base(fixture)
    {
    }

    [IntegrationTest]
    public async Task GetOrganizations_ShouldReturnApprovedOrganizations()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/organizations");

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var organizations = JsonSerializer.Deserialize<List<OrganizationDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        organizations.Should().NotBeNull();
        organizations.Should().AllSatisfy(org =>
            org.Status.Should().Be(OrganizationStatus.Active));
    }

    [IntegrationTest]
    public async Task GetOrganizations_WithPagination_ShouldReturnPagedResults()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/organizations?page=1&pageSize=2");

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [IntegrationTest]
    public async Task GetOrganizations_WithCategoryFilter_ShouldReturnFilteredResults()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/organizations?category={OrganizationCategory.Religia}");

        // Assert
        AssertSuccess(response);
    }

    [IntegrationTest]
    public async Task GetOrganizationById_WithValidId_ShouldReturnOrganization()
    {
        // Arrange
        using var context = Fixture.GetDbContext();
        var organization = context.Organizations.First();

        // Act
        var response = await Client.GetAsync($"/api/v1/organizations/{organization.Id}");

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var orgDto = JsonSerializer.Deserialize<OrganizationDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        orgDto.Should().NotBeNull();
        orgDto.Id.Should().Be(organization.Id);
    }

    [IntegrationTest]
    public async Task GetOrganizationById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/organizations/{invalidId}");

        // Assert
        AssertStatusCode(response, HttpStatusCode.NotFound);
    }

    [IntegrationTest]
    public async Task GetOrganizationStats_WithValidId_ShouldReturnStats()
    {
        // Arrange
        using var context = Fixture.GetDbContext();
        var organization = context.Organizations.First();

        // Act
        var response = await Client.GetAsync($"/api/v1/organizations/{organization.Id}/stats");

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var stats = JsonSerializer.Deserialize<OrganizationStatsDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        stats.Should().NotBeNull();
        stats.TotalDonations.Should().BeGreaterOrEqualTo(0);
        stats.TotalPayments.Should().BeGreaterOrEqualTo(0);
    }

    [IntegrationTest]
    public async Task GetOrganizationQrCode_WithValidId_ShouldReturnImage()
    {
        // Arrange
        using var context = Fixture.GetDbContext();
        var organization = context.Organizations.First();

        // Act
        var response = await Client.GetAsync($"/api/v1/organizations/{organization.Id}/qr");

        // Assert
        AssertSuccess(response);
        response.Content.Headers.ContentType?.MediaType.Should().StartWith("image/");
    }

    [IntegrationTest]
    public async Task GetMyOrganization_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/organization/profile");

        // Assert
        AssertStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [IntegrationTest]
    public async Task GetMyOrganization_WithAuth_ShouldReturnOrganizationProfile()
    {
        // Arrange
        await AuthenticateAsOrganizationAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/organization/profile");

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var organization = JsonSerializer.Deserialize<OrganizationDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        organization.Should().NotBeNull();
        organization.Name.Should().NotBeNullOrEmpty();
    }

    [IntegrationTest]
    public async Task UpdateMyOrganization_WithValidData_ShouldUpdateOrganization()
    {
        // Arrange
        await AuthenticateAsOrganizationAsync();
        
        var updateRequest = new UpdateOrganizationRequest
        {
            Name = "Updated Test Organization",
            Description = "Updated description",
            Category = OrganizationCategory.Edukacja,
            ContactEmail = "updated@test.com",
            ContactPhone = "+48987654321",
            Website = "https://updated.test.com",
            Address = "Updated Address 123",
            City = "Updated City",
            PostalCode = "54-321",
            Country = "Poland",
            BankAccount = "PL61109010140000071219812875"
        };

        // Act
        var response = await Fixture.PutAsync("/api/v1/organization/profile", updateRequest);

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var updatedOrg = JsonSerializer.Deserialize<OrganizationDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        updatedOrg.Should().NotBeNull();
        updatedOrg.Name.Should().Be("Updated Test Organization");
        updatedOrg.Description.Should().Be("Updated description");
        updatedOrg.Category.Should().Be(OrganizationCategory.Edukacja);
    }

    [IntegrationTest]
    public async Task UpdateMyOrganization_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsOrganizationAsync();
        
        var updateRequest = new UpdateOrganizationRequest
        {
            Name = "", // Invalid empty name
            ContactEmail = "invalid-email" // Invalid email format
        };

        // Act
        var response = await Fixture.PutAsync("/api/v1/organization/profile", updateRequest);

        // Assert
        AssertStatusCode(response, HttpStatusCode.BadRequest);
    }

    [IntegrationTest]
    public async Task GetMyDashboardStats_WithAuth_ShouldReturnStats()
    {
        // Arrange
        await AuthenticateAsOrganizationAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/organization/dashboard-stats");

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var stats = JsonSerializer.Deserialize<OrganizationStatsDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        stats.Should().NotBeNull();
    }

    [IntegrationTest]
    public async Task GetMyPayments_WithAuth_ShouldReturnPayments()
    {
        // Arrange
        await AuthenticateAsOrganizationAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/organization/payments");

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [IntegrationTest]
    public async Task GetMyPayments_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        await AuthenticateAsOrganizationAsync();

        // Act
        var response = await Client.GetAsync("/api/v1/organization/payments?page=1&pageSize=5");

        // Assert
        AssertSuccess(response);
    }

    [IntegrationTest]
    public async Task SearchOrganizations_WithValidQuery_ShouldReturnResults()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/organizations/search?q=Test");

        // Assert
        AssertSuccess(response);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [IntegrationTest]
    public async Task SearchOrganizations_WithEmptyQuery_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/organizations/search?q=");

        // Assert
        AssertStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("Organization")]
    [InlineData("Charity")]
    public async Task SearchOrganizations_WithDifferentQueries_ShouldWork(string query)
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/organizations/search?q={query}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [IntegrationTest]
    public async Task UploadLogo_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG header
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "logo", "test.png");

        // Act
        var response = await Client.PostAsync("/api/v1/organization/logo", content);

        // Assert
        AssertStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [IntegrationTest]
    public async Task GetOrganizations_Performance_ShouldRespondQuickly()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/v1/organizations");

        // Assert
        stopwatch.Stop();
        AssertSuccess(response);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // Should respond within 500ms
    }

    [IntegrationTest]
    public async Task GetOrganizations_ConcurrentRequests_ShouldHandleLoad()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Client.GetAsync("/api/v1/organizations"));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.IsSuccessStatusCode.Should().BeTrue());
    }
}