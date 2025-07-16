using CharityPay.Domain.Entities;

namespace CharityPay.Application.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
}