using CharityPay.Domain.Tests.Builders;

namespace CharityPay.Application.Tests.Builders;

// Re-export the domain test builders for convenience
public static class TestDataBuilders
{
    public static UserBuilder User() => new UserBuilder();
    public static OrganizationBuilder Organization() => new OrganizationBuilder();
    public static PaymentBuilder Payment() => new PaymentBuilder();
}