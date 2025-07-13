namespace CharityPay.Application.DTOs.Auth;

/// <summary>
/// Response model for successful login.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// The refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// The authenticated user information.
    /// </summary>
    public UserDto User { get; set; } = null!;
}