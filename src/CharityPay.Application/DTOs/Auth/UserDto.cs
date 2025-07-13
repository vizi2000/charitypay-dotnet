using CharityPay.Domain.Enums;

namespace CharityPay.Application.DTOs.Auth;

/// <summary>
/// User data transfer object.
/// </summary>
public class UserDto
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User role.
    /// </summary>
    public UserRole Role { get; set; }
    
    /// <summary>
    /// Indicates if the user account is active.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// The organization ID if the user is an organization owner.
    /// </summary>
    public Guid? OrganizationId { get; set; }
    
    /// <summary>
    /// The organization name if the user is an organization owner.
    /// </summary>
    public string? OrganizationName { get; set; }
}