using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;
using CharityPay.Domain.ValueObjects;

namespace CharityPay.Infrastructure.Data.Seed;

public class DatabaseSeeder
{
    private readonly CharityPayDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(CharityPayDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Only seed if no organizations exist
            if (await _context.Organizations.AnyAsync())
            {
                _logger.LogInformation("Database already contains organizations, skipping seed");
                return;
            }

            _logger.LogInformation("Seeding database with sample organizations...");

            // Create sample users for organizations
            var users = new List<User>
            {
                User.Create("admin@parafiasw.pl", "hashedPassword", "Admin Parafia", UserRole.Organization),
                User.Create("admin@caritas.pl", "hashedPassword", "Admin Caritas", UserRole.Organization),
                User.Create("admin@parafiamatki.pl", "hashedPassword", "Admin Parafia", UserRole.Organization),
                User.Create("admin@fundacjadzieciom.pl", "hashedPassword", "Admin Fundacja", UserRole.Organization),
                User.Create("admin@schronisko.pl", "hashedPassword", "Admin Schronisko", UserRole.Organization),
                User.Create("admin@hospicjum.pl", "hashedPassword", "Admin Hospicjum", UserRole.Organization)
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Create sample organizations matching demo data
            var organizations = new List<Organization>
            {
                Organization.Create(
                    name: "Parafia Św. Jana",
                    description: "Parafia w centrum miasta. Wspieramy lokalne inicjatywy charytatywne i pomagamy potrzebującym w naszej wspólnocie.",
                    category: "religia",
                    location: "Warszawa",
                    targetAmount: 50000.00m,
                    contactEmail: "kontakt@parafiasw.pl",
                    userId: users[0].Id
                ),
                Organization.Create(
                    name: "Caritas Warszawa",
                    description: "Organizacja charytatywna pomagająca osobom w trudnej sytuacji życiowej. Prowadzimy jadłodajnie, schroniska i programy wsparcia.",
                    category: "inne",
                    location: "Warszawa",
                    targetAmount: 100000.00m,
                    contactEmail: "warszawa@caritas.pl",
                    userId: users[1].Id
                ),
                Organization.Create(
                    name: "Parafia Matki Boskiej",
                    description: "Lokalna parafia organizująca wydarzenia charytatywne dla dzieci i seniorów z naszej dzielnicy.",
                    category: "religia",
                    location: "Kraków",
                    targetAmount: 30000.00m,
                    contactEmail: "kontakt@parafiamatki.pl",
                    userId: users[2].Id
                ),
                Organization.Create(
                    name: "Fundacja Pomocy Dzieciom",
                    description: "Wspieramy dzieci z rodzin w trudnej sytuacji materialnej, organizujemy zajęcia edukacyjne i terapeutyczne.",
                    category: "dzieci",
                    location: "Gdańsk",
                    targetAmount: 40000.00m,
                    contactEmail: "pomoc@fundacjadzieciom.pl",
                    userId: users[3].Id
                ),
                Organization.Create(
                    name: "Schronisko dla Zwierząt",
                    description: "Opiekujemy się bezdomnymi zwierzętami, zapewniamy im opiekę weterynaryjną i szukamy nowych domów.",
                    category: "zwierzeta",
                    location: "Wrocław",
                    targetAmount: 25000.00m,
                    contactEmail: "kontakt@schronisko.pl",
                    userId: users[4].Id
                ),
                Organization.Create(
                    name: "Hospicjum Dobry Samarytanin",
                    description: "Zapewniamy wsparcie osobom terminalnie chorym oraz ich rodzinom.",
                    category: "zdrowie",
                    location: "Poznań",
                    targetAmount: 60000.00m,
                    contactEmail: "kontakt@hospicjum.pl",
                    userId: users[5].Id
                )
            };

            // Set collected amounts to match demo data
            organizations[0].UpdateCollectedAmount(15000.50m);
            organizations[1].UpdateCollectedAmount(25000.00m);
            organizations[2].UpdateCollectedAmount(8500.00m);
            organizations[3].UpdateCollectedAmount(12300.75m);
            organizations[4].UpdateCollectedAmount(6800.25m);
            organizations[5].UpdateCollectedAmount(15500.00m);

            // Update profiles with phone numbers
            organizations[0].UpdateProfile(null, "+48 22 123 45 67", null, null, null, null);
            organizations[1].UpdateProfile(null, "+48 22 987 65 43", null, null, null, null);
            organizations[2].UpdateProfile(null, "+48 12 345 67 89", null, null, null, null);
            organizations[3].UpdateProfile(null, "+48 58 123 45 67", null, null, null, null);
            organizations[4].UpdateProfile(null, "+48 71 234 56 78", null, null, null, null);
            organizations[5].UpdateProfile(null, "+48 61 123 45 67", null, null, null, null);

            // Approve all organizations
            foreach (var org in organizations)
            {
                org.Approve();
            }

            await _context.Organizations.AddRangeAsync(organizations);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} organizations", organizations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }
}