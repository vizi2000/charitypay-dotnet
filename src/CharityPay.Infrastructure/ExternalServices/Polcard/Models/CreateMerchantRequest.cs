namespace CharityPay.Infrastructure.ExternalServices.Polcard.Models;

public record CreateMerchantRequest
{
    public string TemplateId { get; init; } = "992"; // Charity template
    public MerchantDetails Merchant { get; init; } = null!;
    public Address[] Addresses { get; init; } = Array.Empty<Address>();
    public DepositAccount[] Deposits { get; init; } = Array.Empty<DepositAccount>();
    public PersonDetails[] Persons { get; init; } = Array.Empty<PersonDetails>();
}

public record MerchantDetails
{
    public string LegalBusinessName { get; init; } = string.Empty;
    public string DoingBusinessAs { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string BusinessIdTypeCd { get; init; } = "PTIN"; // Polish Tax ID
    public string MerchantCategoryCd { get; init; } = "8398"; // Charitable organizations
    public string WebsiteUrl { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
}

public record Address
{
    public string AddressTypeCd { get; init; } = "LEGAL";
    public string Address1 { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string StateProvinceCd { get; init; } = string.Empty;
    public string PostalCd { get; init; } = string.Empty;
    public string CountryCd { get; init; } = "PL";
}

public record DepositAccount
{
    public string DepositoryNumber { get; init; } = string.Empty; // IBAN
    public string DepositoryTypeCd { get; init; } = "DDA"; // Demand Deposit Account
    public string CurrencyCd { get; init; } = "PLN";
}

public record PersonDetails
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PersonTypeCd { get; init; } = "OWNER";
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}