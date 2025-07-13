using Microsoft.AspNetCore.Mvc;

namespace CharityPay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    [HttpGet("organizations")]
    public IActionResult GetOrganizations()
    {
        var demoOrganizations = new[]
        {
            new 
            { 
                id = 1, 
                name = "Parafia Św. Jana", 
                description = "Parafia w centrum miasta",
                status = "approved",
                collected_amount = 15000.50m
            },
            new 
            { 
                id = 2, 
                name = "Caritas Warszawa", 
                description = "Organizacja charytatywna",
                status = "approved", 
                collected_amount = 25000.00m
            },
            new 
            { 
                id = 3, 
                name = "Parafia Matki Boskiej", 
                description = "Lokalna parafia",
                status = "pending",
                collected_amount = 5000.00m
            }
        };

        return Ok(new { 
            data = demoOrganizations,
            total = demoOrganizations.Length,
            message = "CharityPay .NET API - Demo organizations data"
        });
    }

    [HttpGet("payments")]
    public IActionResult GetPayments()
    {
        var demoPayments = new[]
        {
            new 
            { 
                id = "pay_001", 
                organization_id = 1,
                amount = 100.00m,
                status = "completed",
                payment_method = "card",
                created_at = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new 
            { 
                id = "pay_002", 
                organization_id = 2,
                amount = 250.00m,
                status = "completed",
                payment_method = "blik",
                created_at = DateTimeOffset.UtcNow.AddHours(-3)
            },
            new 
            { 
                id = "pay_003", 
                organization_id = 1,
                amount = 50.00m,
                status = "pending",
                payment_method = "card",
                created_at = DateTimeOffset.UtcNow.AddMinutes(-30)
            }
        };

        return Ok(new { 
            data = demoPayments,
            total = demoPayments.Length,
            message = "CharityPay .NET API - Demo payments data"
        });
    }

    [HttpPost("payments")]
    public IActionResult CreatePayment([FromBody] object paymentRequest)
    {
        var demoPayment = new 
        { 
            id = $"pay_{Guid.NewGuid():N}",
            status = "pending",
            amount = 100.00m,
            payment_url = "https://demo-fiserv.com/payment/12345",
            created_at = DateTimeOffset.UtcNow,
            message = "Demo payment created - this would integrate with Fiserv in production"
        };

        return Ok(demoPayment);
    }
}