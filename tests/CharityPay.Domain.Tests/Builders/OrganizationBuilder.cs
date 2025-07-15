using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Domain.Tests.Builders;

public class OrganizationBuilder
{
    private Organization _organization;

    public OrganizationBuilder()
    {
        _organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = $"Test Organization {Guid.NewGuid():N}",
            Description = "Test organization description",
            ContactEmail = $"contact{Guid.NewGuid():N}@example.com",
            ContactPhone = "+48123456789",
            Status = OrganizationStatus.Pending,
            Category = OrganizationCategory.Inne,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            VerifiedAt = null,
            WebsiteUrl = "https://example.org",
            Address = "Test Street 123",
            City = "Test City",
            PostalCode = "00-000",
            Country = "Poland",
            PrimaryColor = "#007bff",
            SecondaryColor = "#6c757d"
        };
    }

    public OrganizationBuilder WithId(Guid id)
    {
        _organization.Id = id;
        return this;
    }

    public OrganizationBuilder WithName(string name)
    {
        _organization.Name = name;
        return this;
    }

    public OrganizationBuilder WithUser(User user)
    {
        _organization.UserId = user.Id;
        _organization.User = user;
        return this;
    }

    public OrganizationBuilder AsApproved()
    {
        _organization.Status = OrganizationStatus.Approved;
        _organization.VerifiedAt = DateTimeOffset.UtcNow.AddDays(-7);
        return this;
    }

    public OrganizationBuilder AsPending()
    {
        _organization.Status = OrganizationStatus.Pending;
        _organization.VerifiedAt = null;
        return this;
    }

    public OrganizationBuilder AsRejected()
    {
        _organization.Status = OrganizationStatus.Rejected;
        return this;
    }

    public OrganizationBuilder AsInactive()
    {
        _organization.IsActive = false;
        return this;
    }

    public OrganizationBuilder WithCategory(OrganizationCategory category)
    {
        _organization.Category = category;
        return this;
    }

    public OrganizationBuilder WithLocation(string city, string country = "Poland")
    {
        _organization.City = city;
        _organization.Country = country;
        return this;
    }

    public OrganizationBuilder WithBranding(string logoUrl, string primaryColor, string secondaryColor)
    {
        _organization.LogoUrl = logoUrl;
        _organization.PrimaryColor = primaryColor;
        _organization.SecondaryColor = secondaryColor;
        return this;
    }

    public Organization Build() => _organization;
    
    public List<Organization> BuildMany(int count)
    {
        var organizations = new List<Organization>();
        for (int i = 0; i < count; i++)
        {
            organizations.Add(new OrganizationBuilder().Build());
        }
        return organizations;
    }
}