namespace CharityPay.Application.DTOs.Auth;

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// The user's password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}