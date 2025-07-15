using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using CharityPay.Application.Configuration;
using Microsoft.Extensions.Options;

namespace CharityPay.API.Controllers;

[ApiController]
[Route("api/v1/auth-demo")]
public class AuthDemoController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthDemoController> _logger;

    public AuthDemoController(IOptions<JwtSettings> jwtSettings, ILogger<AuthDemoController> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult DemoLogin([FromBody] DemoLoginRequest request)
    {
        _logger.LogInformation("Demo login attempt for: {Email}", request.Email);

        // Demo users
        if (request.Email == "admin@charitypay.pl" && request.Password == "admin123")
        {
            var token = GenerateToken("demo-admin-id", request.Email, "Admin");
            return Ok(new
            {
                access_token = token,
                refresh_token = "demo-refresh-token",
                expires_in = 3600,
                user = new
                {
                    id = "demo-admin-id",
                    email = request.Email,
                    name = "Demo Admin",
                    role = "admin",
                    is_active = true
                }
            });
        }
        else if (request.Email == "org@charitypay.pl" && request.Password == "org123")
        {
            var token = GenerateToken("demo-org-id", request.Email, "Organization");
            return Ok(new
            {
                access_token = token,
                refresh_token = "demo-refresh-token",
                expires_in = 3600,
                user = new
                {
                    id = "demo-org-id",
                    email = request.Email,
                    name = "Demo Organization",
                    role = "organization",
                    is_active = true,
                    organization_id = 1,
                    organization_name = "Parafia Św. Jana"
                }
            });
        }

        return Unauthorized(new { message = "Invalid email or password" });
    }

    [HttpGet("me")]
    public IActionResult GetMe()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            id = userId,
            email = email,
            name = email == "admin@charitypay.pl" ? "Demo Admin" : "Demo Organization",
            role = role?.ToLower(),
            is_active = true,
            organization_id = role == "Organization" ? (int?)1 : null,
            organization_name = role == "Organization" ? "Parafia Św. Jana" : null
        });
    }

    private string GenerateToken(string userId, string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class DemoLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}