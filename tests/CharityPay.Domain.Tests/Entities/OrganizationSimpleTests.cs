using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CharityPay.Domain.Tests.Entities;

public class OrganizationSimpleTests
{
    [Fact]
    public void Organization_Create_ShouldCreateWithValidData()
    {
        // Arrange
        var name = "Test Charity";
        var description = "A test charity organization";
        var category = "Religion";
        var location = "Test City";
        var targetAmount = 10000m;
        var contactEmail = "contact@testcharity.org";
        var userId = Guid.NewGuid();

        // Act
        var organization = Organization.Create(name, description, category, location, targetAmount, contactEmail, userId);

        // Assert
        organization.Should().NotBeNull();
        organization.Id.Should().NotBe(Guid.Empty);
        organization.Name.Should().Be(name);
        organization.Description.Should().Be(description);
        organization.Category.Should().Be(category);
        organization.Location.Should().Be(location);
        organization.TargetAmount.Should().Be(targetAmount);
        organization.ContactEmail.Should().Be(contactEmail);
        organization.UserId.Should().Be(userId);
        organization.Status.Should().Be(OrganizationStatus.Pending);
        organization.CollectedAmount.Should().Be(0);
    }

    [Theory]
    [InlineData(OrganizationStatus.Pending)]
    [InlineData(OrganizationStatus.Active)]
    [InlineData(OrganizationStatus.Rejected)]
    public void Organization_ShouldAllowValidStatuses(OrganizationStatus status)
    {
        // Arrange
        var organization = CreateTestOrganization();

        // Act & Assert based on status
        switch (status)
        {
            case OrganizationStatus.Pending:
                organization.Status.Should().Be(OrganizationStatus.Pending); // Default
                break;
            case OrganizationStatus.Active:
                organization.Approve();
                organization.Status.Should().Be(OrganizationStatus.Active);
                break;
            case OrganizationStatus.Rejected:
                organization.Reject();
                organization.Status.Should().Be(OrganizationStatus.Rejected);
                break;
        }
    }

    [Fact]
    public void Organization_Approve_ShouldChangeStatusToActive()
    {
        // Arrange
        var organization = CreateTestOrganization();
        var adminNotes = "Organization meets all criteria";

        // Act
        organization.Approve(adminNotes);

        // Assert
        organization.Status.Should().Be(OrganizationStatus.Active);
        organization.AdminNotes.Should().Be(adminNotes);
    }

    [Fact]
    public void Organization_Reject_ShouldChangeStatusToRejected()
    {
        // Arrange
        var organization = CreateTestOrganization();
        var adminNotes = "Missing required documentation";

        // Act
        organization.Reject(adminNotes);

        // Assert
        organization.Status.Should().Be(OrganizationStatus.Rejected);
        organization.AdminNotes.Should().Be(adminNotes);
    }

    [Fact]
    public void Organization_UpdateProfile_ShouldUpdateProfileFields()
    {
        // Arrange
        var organization = CreateTestOrganization();
        var website = "https://testcharity.org";
        var phone = "+48123456789";
        var address = "123 Charity St";
        var primaryColor = "#FF0000";
        var secondaryColor = "#0000FF";
        var customMessage = "Thank you for your donation!";

        // Act
        organization.UpdateProfile(website, phone, address, primaryColor, secondaryColor, customMessage);

        // Assert
        organization.Website.Should().Be(website);
        organization.Phone.Should().Be(phone);
        organization.Address.Should().Be(address);
        organization.PrimaryColor.Should().Be(primaryColor);
        organization.SecondaryColor.Should().Be(secondaryColor);
        organization.CustomMessage.Should().Be(customMessage);
    }

    [Fact]
    public void Organization_UpdateLogo_ShouldUpdateLogoUrl()
    {
        // Arrange
        var organization = CreateTestOrganization();
        var logoUrl = "https://example.com/logo.png";

        // Act
        organization.UpdateLogo(logoUrl);

        // Assert
        organization.LogoUrl.Should().Be(logoUrl);
    }

    [Fact]
    public void Organization_UpdateCollectedAmount_ShouldIncreaseAmount()
    {
        // Arrange
        var organization = CreateTestOrganization();
        var additionalAmount = 500m;
        var originalAmount = organization.CollectedAmount;

        // Act
        organization.UpdateCollectedAmount(additionalAmount);

        // Assert
        organization.CollectedAmount.Should().Be(originalAmount + additionalAmount);
    }

    [Fact]
    public void Organization_Payments_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var organization = CreateTestOrganization();

        // Act
        var payments = organization.Payments;

        // Assert
        payments.Should().NotBeNull();
        payments.Should().BeEmpty();
        payments.Should().BeAssignableTo<IReadOnlyCollection<Payment>>();
    }

    private static Organization CreateTestOrganization()
    {
        return Organization.Create(
            "Test Charity",
            "A test charity organization",
            "Religion",
            "Test City",
            10000m,
            "contact@testcharity.org",
            Guid.NewGuid());
    }
}