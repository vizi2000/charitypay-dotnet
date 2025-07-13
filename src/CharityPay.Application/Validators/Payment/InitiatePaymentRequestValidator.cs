using FluentValidation;
using CharityPay.Application.DTOs.Payment;
using CharityPay.Domain.Enums;

namespace CharityPay.Application.Validators.Payment;

/// <summary>
/// Validator for payment initiation requests.
/// </summary>
public class InitiatePaymentRequestValidator : AbstractValidator<InitiatePaymentRequest>
{
    public InitiatePaymentRequestValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(10_000).WithMessage("Amount must not exceed 10,000");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(x => x.DonorName)
            .NotEmpty().WithMessage("Donor name is required")
            .MinimumLength(2).WithMessage("Donor name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("Donor name must not exceed 100 characters");

        RuleFor(x => x.DonorEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.DonorEmail))
            .WithMessage("Invalid email format");

        RuleFor(x => x.DonorPhone)
            .Matches(@"^[\d\s\-\+\(\)]+$").When(x => !string.IsNullOrEmpty(x.DonorPhone))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.ReturnUrl)
            .NotEmpty().WithMessage("Return URL is required")
            .Must(BeValidUrl).WithMessage("Invalid return URL");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}