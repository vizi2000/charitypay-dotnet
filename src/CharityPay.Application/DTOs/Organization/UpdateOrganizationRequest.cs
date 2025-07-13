namespace CharityPay.Application.DTOs.Organization;

/// <summary>
/// Request model for updating organization profile.
/// </summary>
public class UpdateOrganizationRequest
{
    /// <summary>
    /// Organization description.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Website URL.
    /// </summary>
    public string? Website { get; set; }
    
    /// <summary>
    /// Phone number.
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Physical address.
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Primary brand color (hex format).
    /// </summary>
    public string? PrimaryColor { get; set; }
    
    /// <summary>
    /// Secondary brand color (hex format).
    /// </summary>
    public string? SecondaryColor { get; set; }
    
    /// <summary>
    /// Custom thank you message for donors.
    /// </summary>
    public string? CustomMessage { get; set; }
}