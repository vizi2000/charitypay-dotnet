using CharityPay.Domain.Enums;

namespace CharityPay.Application.DTOs.Organization;

/// <summary>
/// Organization data transfer object.
/// </summary>
public class OrganizationDto
{
    /// <summary>
    /// Organization ID.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Organization name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization category.
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
    /// Amount collected so far.
    /// </summary>
    public decimal CollectedAmount { get; set; }
    
    /// <summary>
    /// Percentage of target achieved.
    /// </summary>
    public decimal ProgressPercentage => TargetAmount > 0 ? Math.Round((CollectedAmount / TargetAmount) * 100, 2) : 0;
    
    /// <summary>
    /// Contact email.
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
    /// Optional address.
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Primary brand color.
    /// </summary>
    public string? PrimaryColor { get; set; }
    
    /// <summary>
    /// Secondary brand color.
    /// </summary>
    public string? SecondaryColor { get; set; }
    
    /// <summary>
    /// Custom thank you message.
    /// </summary>
    public string? CustomMessage { get; set; }
    
    /// <summary>
    /// Organization status.
    /// </summary>
    public OrganizationStatus Status { get; set; }
    
    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}