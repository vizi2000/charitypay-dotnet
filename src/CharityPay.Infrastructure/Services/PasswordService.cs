using CharityPay.Application.Services;

namespace CharityPay.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // Secure default work factor

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            // If verification fails due to invalid hash format, return false
            return false;
        }
    }
}