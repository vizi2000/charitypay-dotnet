using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CharityPay.Application.Configuration;
using CharityPay.Application.Services;
using CharityPay.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CharityPay.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly Dictionary<string, DateTime> _refreshTokens = new(); // In production, use a proper store

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        
        // Add organization ID if user has one
        if (user.Organization != null)
        {
            claims.Add(new Claim("org_id", user.Organization.Id.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);
        
        // Store refresh token with expiration
        _refreshTokens[refreshToken] = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        
        return refreshToken;
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var expiration))
            return false;

        if (expiration < DateTime.UtcNow)
        {
            _refreshTokens.Remove(refreshToken);
            return false;
        }

        return true;
    }

    public void InvalidateRefreshToken(string refreshToken)
    {
        _refreshTokens.Remove(refreshToken);
    }

    public (string accessToken, string refreshToken) RefreshTokens(string oldRefreshToken, User user)
    {
        if (!ValidateRefreshToken(oldRefreshToken))
            throw new InvalidOperationException("Invalid refresh token");

        // Remove old refresh token
        _refreshTokens.Remove(oldRefreshToken);

        // Generate new tokens
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        return (newAccessToken, newRefreshToken);
    }
}