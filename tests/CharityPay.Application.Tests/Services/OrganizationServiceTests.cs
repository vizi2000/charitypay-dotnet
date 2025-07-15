using CharityPay.Application.DTOs.Organization;
using CharityPay.Application.Services;
using CharityPay.Application.Tests.Builders;
using CharityPay.Application.Tests.Fixtures;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CharityPay.Application.Tests.Services;

public class OrganizationServiceTests : ApplicationTestBase
{
    private readonly OrganizationService _organizationService;

    public OrganizationServiceTests(ApplicationTestFixture fixture) : base(fixture)
    {
        _organizationService = ServiceTestHelper.CreateOrganizationService(
            DbContext, 
            Mapper, 
            CreateMockLogger<OrganizationService>().Object);
    }

    [UnitTest]
    public async Task GetOrganizationsAsync_ShouldReturnApprovedOrganizations()
    {
        // Arrange
        var approvedOrg1 = TestDataBuilders.Organization().AsApproved().Build();
        var approvedOrg2 = TestDataBuilders.Organization().AsApproved().Build();
        var pendingOrg = TestDataBuilders.Organization().AsPending().Build();
        
        DbContext.Organizations.AddRange(approvedOrg1, approvedOrg2, pendingOrg);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.GetOrganizationsAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(org => 
            org.Status.Should().Be(OrganizationStatus.Approved));
    }

    [UnitTest]
    public async Task GetOrganizationsAsync_ShouldRespectPagination()
    {
        // Arrange
        var organizations = TestDataBuilders.Organization()
            .AsApproved()
            .BuildMany(15);
        
        DbContext.Organizations.AddRange(organizations);
        await DbContext.SaveChangesAsync();

        // Act
        var page1 = await _organizationService.GetOrganizationsAsync(1, 5);
        var page2 = await _organizationService.GetOrganizationsAsync(2, 5);

        // Assert
        page1.Items.Should().HaveCount(5);
        page2.Items.Should().HaveCount(5);
        page1.TotalCount.Should().Be(15);
        page2.TotalCount.Should().Be(15);
        page1.CurrentPage.Should().Be(1);
        page2.CurrentPage.Should().Be(2);
        
        // Should not have overlapping items
        var page1Ids = page1.Items.Select(o => o.Id).ToList();
        var page2Ids = page2.Items.Select(o => o.Id).ToList();
        page1Ids.Should().NotIntersectWith(page2Ids);
    }

    [UnitTest]
    public async Task GetOrganizationByIdAsync_WithValidId_ShouldReturnOrganization()
    {
        // Arrange
        var organization = TestDataBuilders.Organization()
            .AsApproved()
            .WithName("Test Charity")
            .Build();
        
        DbContext.Organizations.Add(organization);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.GetOrganizationByIdAsync(organization.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(organization.Id);
        result.Name.Should().Be("Test Charity");
        result.Status.Should().Be(OrganizationStatus.Approved);
    }

    [UnitTest]
    public async Task GetOrganizationByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _organizationService.GetOrganizationByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [UnitTest]
    public async Task GetMyOrganizationAsync_WithValidUserId_ShouldReturnOrganization()
    {
        // Arrange
        var user = TestDataBuilders.User().AsOrganization().Build();
        var organization = TestDataBuilders.Organization()
            .WithUser(user)
            .Build();
        
        DbContext.Users.Add(user);
        DbContext.Organizations.Add(organization);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.GetMyOrganizationAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(organization.Id);
        result.UserId.Should().Be(user.Id);
    }

    [UnitTest]
    public async Task GetMyOrganizationAsync_WithInvalidUserId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentUserId = "non-existent-user";

        // Act
        var result = await _organizationService.GetMyOrganizationAsync(nonExistentUserId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [UnitTest]
    public async Task UpdateOrganizationAsync_WithValidData_ShouldUpdateOrganization()
    {
        // Arrange
        var user = TestDataBuilders.User().AsOrganization().Build();
        var organization = TestDataBuilders.Organization()
            .WithUser(user)
            .WithName("Original Name")
            .Build();
        
        DbContext.Users.Add(user);
        DbContext.Organizations.Add(organization);
        await DbContext.SaveChangesAsync();

        var updateRequest = new UpdateOrganizationRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Category = OrganizationCategory.Edukacja,
            ContactEmail = "updated@example.com",
            ContactPhone = "+48987654321",
            Website = "https://updated.example.com",
            Address = "Updated Address",
            City = "Updated City",
            PostalCode = "54-321",
            Country = "Poland",
            BankAccount = "PL61109010140000071219812875"
        };

        // Act
        var result = await _organizationService.UpdateOrganizationAsync(organization.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Category.Should().Be(OrganizationCategory.Edukacja);
        result.ContactEmail.Should().Be("updated@example.com");

        // Verify database was updated
        var updatedOrg = await DbContext.Organizations.FindAsync(organization.Id);
        updatedOrg.Name.Should().Be("Updated Name");
        updatedOrg.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [UnitTest]
    public async Task UpdateOrganizationAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateOrganizationRequest
        {
            Name = "Test Name"
        };

        // Act
        var result = await _organizationService.UpdateOrganizationAsync(nonExistentId, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [UnitTest]
    public async Task GetOrganizationStatsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var organization = TestDataBuilders.Organization().AsApproved().Build();
        var payments = new[]
        {
            TestDataBuilders.Payment().WithOrganization(organization).WithAmount(100m).AsCompleted().Build(),
            TestDataBuilders.Payment().WithOrganization(organization).WithAmount(200m).AsCompleted().Build(),
            TestDataBuilders.Payment().WithOrganization(organization).WithAmount(150m).AsPending().Build(),
            TestDataBuilders.Payment().WithOrganization(organization).WithAmount(50m).AsFailed().Build()
        };

        DbContext.Organizations.Add(organization);
        DbContext.Payments.AddRange(payments);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.GetOrganizationStatsAsync(organization.Id);

        // Assert
        result.Should().NotBeNull();
        result.TotalDonations.Should().Be(300m); // Only completed payments
        result.TotalPayments.Should().Be(2); // Only completed payments
        result.TotalDonors.Should().BeGreaterThan(0);
        result.LastPaymentDate.Should().NotBeNull();
    }

    [UnitTest]
    public async Task GetOrganizationStatsAsync_WithNoPayments_ShouldReturnZeroStats()
    {
        // Arrange
        var organization = TestDataBuilders.Organization().AsApproved().Build();
        DbContext.Organizations.Add(organization);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.GetOrganizationStatsAsync(organization.Id);

        // Assert
        result.Should().NotBeNull();
        result.TotalDonations.Should().Be(0m);
        result.TotalPayments.Should().Be(0);
        result.TotalDonors.Should().Be(0);
        result.LastPaymentDate.Should().BeNull();
    }

    [UnitTest]
    public async Task GetOrganizationStatsAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _organizationService.GetOrganizationStatsAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(OrganizationCategory.Religia)]
    [InlineData(OrganizationCategory.Dzieci)]
    [InlineData(OrganizationCategory.Zwierzeta)]
    public async Task GetOrganizationsAsync_WithCategoryFilter_ShouldReturnFilteredResults(OrganizationCategory category)
    {
        // Arrange
        var religiaOrg = TestDataBuilders.Organization().AsApproved().WithCategory(OrganizationCategory.Religia).Build();
        var dzieciOrg = TestDataBuilders.Organization().AsApproved().WithCategory(OrganizationCategory.Dzieci).Build();
        var zwierzetaOrg = TestDataBuilders.Organization().AsApproved().WithCategory(OrganizationCategory.Zwierzeta).Build();
        
        DbContext.Organizations.AddRange(religiaOrg, dzieciOrg, zwierzetaOrg);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.GetOrganizationsAsync(1, 10, category);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Category.Should().Be(category);
    }

    [UnitTest]
    public async Task SearchOrganizationsAsync_ShouldFindByName()
    {
        // Arrange
        var org1 = TestDataBuilders.Organization().AsApproved().WithName("Animal Rescue Foundation").Build();
        var org2 = TestDataBuilders.Organization().AsApproved().WithName("Children's Hospital Fund").Build();
        var org3 = TestDataBuilders.Organization().AsApproved().WithName("Animal Welfare Society").Build();
        
        DbContext.Organizations.AddRange(org1, org2, org3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.SearchOrganizationsAsync("Animal", 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(org => 
            org.Name.Should().Contain("Animal"));
    }

    [UnitTest]
    public async Task GetOrganizationPaymentsAsync_ShouldReturnOrganizationPayments()
    {
        // Arrange
        var organization = TestDataBuilders.Organization().AsApproved().Build();
        var otherOrganization = TestDataBuilders.Organization().AsApproved().Build();
        
        var orgPayments = TestDataBuilders.Payment().WithOrganization(organization).BuildMany(3);
        var otherPayments = TestDataBuilders.Payment().WithOrganization(otherOrganization).BuildMany(2);

        DbContext.Organizations.AddRange(organization, otherOrganization);
        DbContext.Payments.AddRange(orgPayments.Concat(otherPayments));
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.GetOrganizationPaymentsAsync(organization.Id, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.Items.Should().AllSatisfy(payment => 
            payment.OrganizationId.Should().Be(organization.Id));
    }

    [UnitTest]
    public async Task IsOrganizationActiveAsync_WithActiveOrganization_ShouldReturnTrue()
    {
        // Arrange
        var organization = TestDataBuilders.Organization()
            .AsApproved()
            .Build(); // Default is active

        DbContext.Organizations.Add(organization);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.IsOrganizationActiveAsync(organization.Id);

        // Assert
        result.Should().BeTrue();
    }

    [UnitTest]
    public async Task IsOrganizationActiveAsync_WithInactiveOrganization_ShouldReturnFalse()
    {
        // Arrange
        var organization = TestDataBuilders.Organization()
            .AsApproved()
            .AsInactive()
            .Build();

        DbContext.Organizations.Add(organization);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _organizationService.IsOrganizationActiveAsync(organization.Id);

        // Assert
        result.Should().BeFalse();
    }

    [UnitTest]
    public async Task IsOrganizationActiveAsync_WithNonExistentOrganization_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _organizationService.IsOrganizationActiveAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }
}