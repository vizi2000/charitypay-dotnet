var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CharityPay API", Version = "v1" });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

// Add welcome endpoint
app.MapGet("/", () => new
{
    message = "Welcome to CharityPay .NET API",
    version = "1.0.0",
    status = "running",
    timestamp = DateTimeOffset.UtcNow,
    environment = app.Environment.EnvironmentName,
    endpoints = new[]
    {
        "/api/demo/organizations - Demo organizations",
        "/api/demo/payments - Demo payments", 
        "/health - Health check",
        "/swagger - API documentation"
    }
});

// Health endpoint is handled by HealthController

app.Run();