using Microsoft.AspNetCore.Mvc;

namespace CharityPay.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTimeOffset.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }

    [HttpGet("detailed")]
    public IActionResult GetDetailed()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTimeOffset.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            uptime = Environment.TickCount64,
            machineName = Environment.MachineName,
            dotnetVersion = Environment.Version.ToString()
        });
    }
}