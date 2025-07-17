using Microsoft.AspNetCore.Mvc;

namespace CharityPay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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
                description = "Parafia w centrum miasta. Wspieramy lokalne inicjatywy charytatywne i pomagamy potrzebującym w naszej wspólnocie.",
                status = "approved",
                collected_amount = 15000.50m,
                target_amount = 50000.00m,
                category = "religia",
                location = "Warszawa",
                logo = (string?)null,
                created_at = DateTimeOffset.UtcNow.AddDays(-30)
            },
            new 
            { 
                id = 2, 
                name = "Caritas Warszawa", 
                description = "Organizacja charytatywna pomagająca osobom w trudnej sytuacji życiowej. Prowadzimy jadłodajnie, schroniska i programy wsparcia.",
                status = "approved", 
                collected_amount = 25000.00m,
                target_amount = 100000.00m,
                category = "inne",
                location = "Warszawa",
                logo = (string?)null,
                created_at = DateTimeOffset.UtcNow.AddDays(-45)
            },
            new 
            { 
                id = 3, 
                name = "Parafia Matki Boskiej", 
                description = "Lokalna parafia organizująca wydarzenia charytatywne dla dzieci i seniorów z naszej dzielnicy.",
                status = "approved",
                collected_amount = 8500.00m,
                target_amount = 30000.00m,
                category = "religia",
                location = "Kraków",
                logo = (string?)null,
                created_at = DateTimeOffset.UtcNow.AddDays(-15)
            },
            new 
            { 
                id = 4, 
                name = "Fundacja Pomocy Dzieciom", 
                description = "Wspieramy dzieci z rodzin w trudnej sytuacji materialnej, organizujemy zajęcia edukacyjne i terapeutyczne.",
                status = "approved",
                collected_amount = 12300.75m,
                target_amount = 40000.00m,
                category = "dzieci",
                location = "Gdańsk",
                logo = (string?)null,
                created_at = DateTimeOffset.UtcNow.AddDays(-20)
            },
            new 
            { 
                id = 5, 
                name = "Schronisko dla Zwierząt", 
                description = "Opiekujemy się bezdomnymi zwierzętami, zapewniamy im opiekę weterynaryjną i szukamy nowych domów.",
                status = "approved",
                collected_amount = 6800.25m,
                target_amount = 25000.00m,
                category = "zwierzeta",
                location = "Wrocław",
                logo = (string?)null,
                created_at = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new
            {
                id = 6,
                name = "Hospicjum Dobry Samarytanin",
                description = "Zapewniamy wsparcie osobom terminalnie chorym oraz ich rodzinom.",
                status = "approved",
                collected_amount = 15500.00m,
                target_amount = 60000.00m,
                category = "zdrowie",
                location = "Poznań",
                logo = (string?)null,
                created_at = DateTimeOffset.UtcNow.AddDays(-5)
            }
        };

        return Ok(demoOrganizations);
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