# CharityPay .NET API Dockerfile
# Multi-stage build for optimal production image

# ============================================================================
# Development Stage
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app

# Copy project files
COPY *.sln ./
COPY src/CharityPay.Domain/*.csproj ./src/CharityPay.Domain/
COPY src/CharityPay.Application/*.csproj ./src/CharityPay.Application/
COPY src/CharityPay.Infrastructure/*.csproj ./src/CharityPay.Infrastructure/
COPY src/CharityPay.API/*.csproj ./src/CharityPay.API/
COPY tests/CharityPay.Domain.Tests/*.csproj ./tests/CharityPay.Domain.Tests/
COPY tests/CharityPay.Application.Tests/*.csproj ./tests/CharityPay.Application.Tests/
COPY tests/CharityPay.Infrastructure.Tests/*.csproj ./tests/CharityPay.Infrastructure.Tests/
COPY tests/CharityPay.API.Tests/*.csproj ./tests/CharityPay.API.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Development entry point
WORKDIR /app/src/CharityPay.API
EXPOSE 80
EXPOSE 443

CMD ["dotnet", "run", "--urls", "http://+:80"]

# ============================================================================
# Build Stage
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY *.sln ./
COPY src/CharityPay.Domain/*.csproj ./src/CharityPay.Domain/
COPY src/CharityPay.Application/*.csproj ./src/CharityPay.Application/
COPY src/CharityPay.Infrastructure/*.csproj ./src/CharityPay.Infrastructure/
COPY src/CharityPay.API/*.csproj ./src/CharityPay.API/
COPY tests/CharityPay.Domain.Tests/*.csproj ./tests/CharityPay.Domain.Tests/
COPY tests/CharityPay.Application.Tests/*.csproj ./tests/CharityPay.Application.Tests/
COPY tests/CharityPay.Infrastructure.Tests/*.csproj ./tests/CharityPay.Infrastructure.Tests/
COPY tests/CharityPay.API.Tests/*.csproj ./tests/CharityPay.API.Tests/

# Restore packages
RUN dotnet restore

# Copy all source code
COPY . .

# Run tests
RUN dotnet test --configuration Release --no-restore --verbosity normal

# Build and publish
WORKDIR /src/src/CharityPay.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# ============================================================================
# Production Runtime Stage
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Install necessary packages
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create uploads directory and set permissions
RUN mkdir -p /app/uploads && \
    chown -R appuser:appuser /app && \
    chmod -R 755 /app

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "CharityPay.API.dll"]

# ============================================================================
# Test Stage (for CI/CD)
# ============================================================================
FROM build AS test
WORKDIR /src

# Install test reporting tools
RUN dotnet tool install --global dotnet-reportgenerator-globaltool
ENV PATH="${PATH}:/root/.dotnet/tools"

# Run all tests with coverage
RUN dotnet test --configuration Release --no-restore \
    --collect:"XPlat Code Coverage" \
    --results-directory /testresults \
    --logger "trx;LogFileName=testresults.trx" \
    --verbosity normal

# Generate coverage report
RUN reportgenerator \
    "-reports:/testresults/**/coverage.cobertura.xml" \
    "-targetdir:/coverage" \
    "-reporttypes:Html;Cobertura"

# ============================================================================
# Documentation Stage
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS docs
WORKDIR /src

# Copy source for documentation generation
COPY . .

# Generate API documentation (if tools are added later)
RUN dotnet restore

# Build for documentation extraction
RUN dotnet build --configuration Release --no-restore