namespace CharityPay.Application.DTOs.Auth;

/// <summary>
/// Request model for organization registration.
/// </summary>
public class RegisterOrganizationRequest
{
    /// <summary>
    /// User account email.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User account password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Confirm password field.
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization category (e.g., Religia, Dzieci, Edukacja).
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization location.
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Target fundraising amount.
    /// </summary>
    public decimal TargetAmount { get; set; }
    
    /// <summary>
    /// Contact email for the organization.
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional website URL.
    /// </summary>
    public string? Website { get; set; }
    
    /// <summary>
    /// Optional phone number.
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Optional physical address.
    /// </summary>
    public string? Address { get; set; }
}