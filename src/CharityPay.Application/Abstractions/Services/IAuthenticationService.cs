using CharityPay.Application.DTOs.Auth;

namespace CharityPay.Application.Abstractions.Services;

/// <summary>
/// Authentication service interface.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registers a new organization with user account.
    /// </summary>
    Task<LoginResponse> RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current authenticated user information.
    /// </summary>
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}