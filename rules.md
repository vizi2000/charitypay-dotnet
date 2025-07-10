# CharityPay .NET - Development Rules & Guidelines

This document outlines the rules, conventions, and best practices for the CharityPay .NET project. All developers must adhere to these guidelines to ensure code quality, consistency, and maintainability.

## Core Principles

### AI Assistant Behavior & Safety
- **Never assume missing context.** Ask questions if uncertain.
- **Never hallucinate packages or APIs** – only use verified .NET packages and APIs.
- **Always confirm file paths and namespaces** exist before referencing them.
- **Never delete or overwrite existing code** unless explicitly instructed or part of a task.
- **Never hardcode secrets** – use configuration providers and user secrets.
- **One class per file** – maintain clear file organization.

### Development Process
- **Test-Driven Development (TDD)** preferred for new features.
- **Pull Request workflow** with code reviews required.
- **Feature branches** named as `feature/CS-XXX-description`.
- **Conventional Commits** with task references (e.g., `feat(api): Add payment endpoint (CS-079)`).
- **Update TASK.md** immediately when starting or completing tasks.

## C# Coding Standards

### General Guidelines
- **Target Framework**: .NET 8.0
- **Language Version**: C# 12
- **Nullable Reference Types**: Enabled project-wide
- **Implicit Usings**: Enabled for common namespaces
- **File-Scoped Namespaces**: Preferred over block-scoped

### Naming Conventions

```csharp
// Interfaces - prefix with 'I'
public interface IPaymentService { }

// Classes - PascalCase
public class PaymentService { }

// Methods - PascalCase
public async Task<Payment> ProcessPaymentAsync() { }

// Properties - PascalCase
public string OrganizationName { get; set; }

// Private fields - camelCase with underscore
private readonly ILogger<PaymentService> _logger;

// Constants - UPPER_CASE
private const int MAX_RETRY_COUNT = 3;

// Local variables - camelCase
var paymentResult = await ProcessPayment();

// Parameters - camelCase
public void UpdateStatus(string paymentId, PaymentStatus newStatus) { }
```

### Code Organization

```csharp
namespace CharityPay.Domain.Entities;

// 1. Using statements (sorted and grouped)
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using CharityPay.Domain.Enums;

// 2. File-scoped namespace
public class Organization
{
    // 3. Constants
    private const int MaxNameLength = 100;
    
    // 4. Fields
    private readonly List<Payment> _payments;
    
    // 5. Constructors
    public Organization(string name)
    {
        Name = name;
        _payments = new List<Payment>();
    }
    
    // 6. Properties
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    
    // 7. Public methods
    public void AddPayment(Payment payment) { }
    
    // 8. Private methods
    private void ValidateName(string name) { }
}
```

### Async/Await Best Practices

```csharp
// Always use Async suffix for async methods
public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
{
    // Always include CancellationToken
    return await ProcessPaymentAsync(request, CancellationToken.None);
}

// Avoid async void except for event handlers
public async Task HandleWebhookAsync() // Good
{
    await ProcessWebhook();
}

// ConfigureAwait(false) in library code
await SomeOperationAsync().ConfigureAwait(false);

// Parallel async operations
var tasks = payments.Select(p => ProcessPaymentAsync(p));
var results = await Task.WhenAll(tasks);
```

### Exception Handling

```csharp
// Create domain-specific exceptions
public class PaymentProcessingException : Exception
{
    public string PaymentId { get; }
    
    public PaymentProcessingException(string paymentId, string message, Exception innerException = null)
        : base(message, innerException)
    {
        PaymentId = paymentId;
    }
}

// Use guard clauses
public void ProcessPayment(Payment payment)
{
    ArgumentNullException.ThrowIfNull(payment);
    
    if (payment.Amount <= 0)
        throw new ArgumentException("Payment amount must be positive", nameof(payment));
}

// Global exception handling in middleware
app.UseExceptionHandler(app =>
{
    app.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        // Handle and log exception
    });
});
```

### Dependency Injection

```csharp
// Register services with appropriate lifetimes
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
builder.Services.AddTransient<IEmailService, EmailService>();

// Use constructor injection
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<PaymentService> _logger;
    
    public PaymentService(
        IPaymentRepository repository,
        ILogger<PaymentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// Avoid service locator pattern
// BAD: var service = serviceProvider.GetService<IPaymentService>();
```

### LINQ Best Practices

```csharp
// Use method syntax for complex queries
var activeOrganizations = organizations
    .Where(o => o.Status == OrganizationStatus.Approved)
    .OrderBy(o => o.Name)
    .Select(o => new OrganizationDto
    {
        Id = o.Id,
        Name = o.Name
    })
    .ToListAsync();

// Use Any() instead of Count() > 0
if (payments.Any(p => p.Status == PaymentStatus.Pending)) // Good

// Use FirstOrDefault with predicate
var payment = await payments.FirstOrDefaultAsync(p => p.Id == paymentId);

// Avoid multiple enumeration
var paymentsList = payments.ToList(); // Enumerate once
var count = paymentsList.Count;
var sum = paymentsList.Sum(p => p.Amount);
```

## Architecture Guidelines

### Clean Architecture Rules

1. **Dependency Direction**: Dependencies point inward (API → Application → Domain)
2. **Domain Independence**: Domain layer has no external dependencies
3. **Interface Segregation**: Small, focused interfaces
4. **Single Responsibility**: Each class has one reason to change

### Entity Framework Core

```csharp
// Configurations in separate files
public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(o => o.Status);
        
        builder.HasMany(o => o.Payments)
            .WithOne(p => p.Organization)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// Avoid lazy loading
// Use explicit loading or eager loading with Include()
var organization = await context.Organizations
    .Include(o => o.Payments)
    .FirstOrDefaultAsync(o => o.Id == id);
```

### API Design

```csharp
// RESTful endpoints with proper HTTP verbs
app.MapGet("/api/v1/organizations", GetOrganizations);
app.MapGet("/api/v1/organizations/{id}", GetOrganizationById);
app.MapPost("/api/v1/organizations", CreateOrganization);
app.MapPut("/api/v1/organizations/{id}", UpdateOrganization);
app.MapDelete("/api/v1/organizations/{id}", DeleteOrganization);

// Consistent response format
public record ApiResponse<T>(
    bool Success,
    T Data,
    string Message = null,
    Dictionary<string, string[]> Errors = null);

// Proper status codes
return Results.Ok(new ApiResponse<OrganizationDto>(true, organizationDto));
return Results.BadRequest(new ApiResponse<object>(false, null, "Invalid request", errors));
return Results.NotFound(new ApiResponse<object>(false, null, "Organization not found"));
```

### Validation

```csharp
// FluentValidation rules
public class CreateOrganizationValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(3, 100).WithMessage("Name must be between 3 and 100 characters");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
            
        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be positive");
    }
}
```

## Testing Standards

### Unit Testing

```csharp
// xUnit with descriptive test names
public class PaymentServiceTests
{
    [Fact]
    public async Task ProcessPayment_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var mockRepository = new Mock<IPaymentRepository>();
        var service = new PaymentService(mockRepository.Object);
        var request = new PaymentRequest { Amount = 100 };
        
        // Act
        var result = await service.ProcessPaymentAsync(request);
        
        // Assert
        Assert.True(result.Success);
        mockRepository.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task ProcessPayment_WithInvalidAmount_ThrowsException(decimal amount)
    {
        // Test implementation
    }
}
```

### Integration Testing

```csharp
// WebApplicationFactory for API testing
public class OrganizationEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    [Fact]
    public async Task GetOrganizations_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/v1/organizations");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("organizations", content);
    }
}
```

## Security Guidelines

### Authentication & Authorization

```csharp
// JWT configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", 
        policy => policy.RequireRole("Admin"));
        
    options.AddPolicy("RequireOrganizationOwner",
        policy => policy.Requirements.Add(new OrganizationOwnerRequirement()));
});
```

### Data Protection

```csharp
// Never log sensitive data
_logger.LogInformation("Payment initiated for organization {OrganizationId}", 
    payment.OrganizationId); // Good

// Use data protection for sensitive data
public class EncryptionService
{
    private readonly IDataProtector _protector;
    
    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }
}
```

## Performance Guidelines

### Async Operations

```csharp
// Use async for I/O operations
public async Task<IEnumerable<Organization>> GetOrganizationsAsync()
{
    return await _context.Organizations
        .AsNoTracking() // Read-only queries
        .Where(o => o.Status == OrganizationStatus.Approved)
        .ToListAsync();
}

// Batch operations
public async Task UpdatePaymentsAsync(IEnumerable<Payment> payments)
{
    _context.Payments.UpdateRange(payments);
    await _context.SaveChangesAsync();
}
```

### Caching

```csharp
// Memory cache for frequently accessed data
public async Task<Organization> GetOrganizationAsync(Guid id)
{
    var cacheKey = $"organization_{id}";
    
    if (!_cache.TryGetValue(cacheKey, out Organization organization))
    {
        organization = await _repository.GetByIdAsync(id);
        
        if (organization != null)
        {
            _cache.Set(cacheKey, organization, TimeSpan.FromMinutes(5));
        }
    }
    
    return organization;
}
```

## Documentation Standards

### XML Documentation

```csharp
/// <summary>
/// Processes a payment request through the Fiserv gateway.
/// </summary>
/// <param name="request">The payment request containing amount and method.</param>
/// <param name="cancellationToken">Cancellation token for the operation.</param>
/// <returns>A task representing the payment result.</returns>
/// <exception cref="PaymentProcessingException">
/// Thrown when the payment gateway returns an error.
/// </exception>
public async Task<PaymentResult> ProcessPaymentAsync(
    PaymentRequest request, 
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### README Updates

- Update README.md when adding new features
- Document all configuration options
- Include examples for common scenarios
- Keep setup instructions current

## Git Workflow

### Branch Naming
- `main` - Production-ready code
- `develop` - Integration branch
- `feature/CS-XXX-description` - Feature branches
- `bugfix/CS-XXX-description` - Bug fix branches
- `hotfix/CS-XXX-description` - Emergency fixes

### Commit Messages
```
feat(api): Add organization approval endpoint (CS-086)

- Implement PUT /api/v1/admin/organizations/{id}/approve
- Add authorization policy for admin-only access
- Include email notification on approval
```

### Pull Request Template
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Tasks Completed
- [ ] CS-XXX: Task description

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project conventions
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No hardcoded values
```

## Monitoring & Logging

### Structured Logging

```csharp
// Use structured logging with Serilog
Log.Information("Payment {PaymentId} processed successfully for {Amount:C}", 
    paymentId, amount);

Log.Error(ex, "Failed to process payment {PaymentId}", paymentId);

// Correlation IDs for request tracking
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["UserId"] = userId
}))
{
    // Operations within this scope include the correlation ID
}
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CharityPayDbContext>()
    .AddUrlGroup(new Uri("https://api.fiserv.com/health"), "Fiserv API")
    .AddCheck<CustomHealthCheck>("Custom Check");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## Final Notes

- **Code reviews are mandatory** - No direct commits to main/develop
- **Keep methods small** - Single responsibility principle
- **Avoid premature optimization** - Profile first
- **Document decisions** - Use ADRs for architectural decisions
- **Stay updated** - Follow .NET announcements and best practices