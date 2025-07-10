using Serilog;

namespace CharityPay.API;

/// <summary>
/// Main entry point for the CharityPay API application.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static async Task<int> Main(string[] args)
    {
        // Configure Serilog early for startup logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/startup-.log", rollingInterval: RollingInterval.Day)
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting CharityPay API");

            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());

            // Add services to the container
            ConfigureServices(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            ConfigurePipeline(app);

            Log.Information("CharityPay API configured successfully");

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "CharityPay API terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Configure application services.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add basic services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Add Swagger/OpenAPI
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() 
            { 
                Title = "CharityPay API", 
                Version = "v1",
                Description = "Enterprise-grade charitable payment platform API"
            });
        });

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Add health checks
        builder.Services.AddHealthChecks();
    }

    /// <summary>
    /// Configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The web application.</param>
    private static void ConfigurePipeline(WebApplication app)
    {
        // Configure for development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CharityPay API v1");
                c.RoutePrefix = "swagger";
            });
        }

        // Add security headers
        app.UseHttpsRedirection();
        app.UseCors();

        // Add request logging
        app.UseSerilogRequestLogging();

        // Add routing
        app.UseRouting();

        // Map endpoints
        app.MapControllers();
        app.MapHealthChecks("/health");

        // Root endpoint
        app.MapGet("/", () => new
        {
            Message = "CharityPay .NET API - MVP",
            Version = "1.0.0",
            Status = "running",
            Timestamp = DateTime.UtcNow
        });
    }
}