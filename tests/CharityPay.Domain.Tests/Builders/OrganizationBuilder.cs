using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Domain.Tests.Builders;

public class OrganizationBuilder
{
    private string _name = "Test Organization";
    private string _description = "Test description";
    private string _category = "Inne";
    private string _location = "Test City";
    private decimal _targetAmount = 1000m;
    private string _contactEmail = "contact@example.com";
    private Guid _userId = Guid.NewGuid();

    public OrganizationBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public OrganizationBuilder WithUser(User user)
    {
        _userId = user.Id;
        return this;
    }

    public OrganizationBuilder AsActive()
    {
        _status = OrganizationStatus.Active;
        return this;
    }

    public OrganizationBuilder AsPending()
    {
        _status = OrganizationStatus.Pending;
        return this;
    }

    public OrganizationBuilder AsRejected()
    {
        _status = OrganizationStatus.Rejected;
        return this;
    }

    private OrganizationStatus _status = OrganizationStatus.Pending;

    public Organization Build()
    {
        var organization = Organization.Create(_name, _description, _category, _location, _targetAmount, _contactEmail, _userId);
        if (_status == OrganizationStatus.Active)
        {
            organization.Approve();
        }
        else if (_status == OrganizationStatus.Rejected)
        {
            organization.Reject();
        }
        return organization;
    }

    public List<Organization> BuildMany(int count)
    {
        var list = new List<Organization>();
        for (int i = 0; i < count; i++)
        {
            list.Add(Build());
        }
        return list;
    }
}
