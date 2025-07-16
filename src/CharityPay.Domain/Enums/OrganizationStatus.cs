namespace CharityPay.Domain.Enums;

public enum OrganizationStatus
{
    Pending = 0,
    KycSubmitted = 1,
    MerchantApproved = 2,
    Active = 3,
    Rejected = 4,
    Suspended = 5
}