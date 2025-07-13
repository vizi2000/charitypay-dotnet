using CharityPay.Domain.Entities;

namespace CharityPay.Application.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateRefreshToken(string refreshToken);
    (string accessToken, string refreshToken) RefreshTokens(string oldRefreshToken, User user);
}