namespace CharityPay.Domain.Tests.Builders;

public static class TestDataBuilders
{
    public static UserBuilder User() => new UserBuilder();
    public static OrganizationBuilder Organization() => new OrganizationBuilder();
    public static PaymentBuilder Payment() => new PaymentBuilder();
}