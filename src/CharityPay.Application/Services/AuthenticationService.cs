using AutoMapper;
using Microsoft.Extensions.Logging;
using CharityPay.Application.Abstractions;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Application.DTOs.Auth;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Application.Services;

/// <summary>
/// Implementation of authentication service.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    
    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AuthenticationService> logger,
        IJwtService jwtService,
        IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }
    
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is inactive");
        }
        
        var token = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        var userDto = _mapper.Map<UserDto>(user);
        
        return new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hour
            User = userDto
        };
    }
    
    public async Task<LoginResponse> RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already registered");
        }
        
        // Create user
        var passwordHash = _passwordService.HashPassword(request.Password);
        var user = User.Create(request.Email, passwordHash, request.Email, UserRole.Organization);
        
        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        
        // Create organization
        var organization = Organization.Create(
            request.OrganizationName,
            request.Description,
            request.Category,
            request.Location,
            request.TargetAmount,
            request.ContactEmail,
            user.Id
        );
        
        if (!string.IsNullOrEmpty(request.Website))
            organization.UpdateProfile(request.Website, request.Phone, request.Address, null, null, null);
        
        await _unitOfWork.Organizations.AddAsync(organization, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("New organization registered: {OrganizationName} by {Email}", 
            request.OrganizationName, request.Email);
        
        // Auto-login after registration
        var loginRequest = new LoginRequest { Email = request.Email, Password = request.Password };
        return await LoginAsync(loginRequest, cancellationToken);
    }
    
    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (!_jwtService.ValidateRefreshToken(refreshToken))
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }
        
        // TODO: Get user from refresh token store
        throw new NotImplementedException("Refresh token user lookup not yet implemented");
    }
    
    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }
        
        return _mapper.Map<UserDto>(user);
    }
    
    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // TODO: Implement refresh token invalidation
        await Task.CompletedTask;
        _logger.LogInformation("User logged out");
    }
    
}