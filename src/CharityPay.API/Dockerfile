# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ../../*.sln ./
COPY ../../*/CharityPay.Domain/*.csproj ./CharityPay.Domain/
COPY ../../*/CharityPay.Application/*.csproj ./CharityPay.Application/
COPY ../../*/CharityPay.Infrastructure/*.csproj ./CharityPay.Infrastructure/
COPY ../../*/CharityPay.API/*.csproj ./CharityPay.API/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY ../../*/CharityPay.Domain/* ./CharityPay.Domain/
COPY ../../*/CharityPay.Application/* ./CharityPay.Application/
COPY ../../*/CharityPay.Infrastructure/* ./CharityPay.Infrastructure/
COPY ../../*/CharityPay.API/* ./CharityPay.API/

# Build and publish
WORKDIR /src/CharityPay.API
RUN dotnet publish -c Release -o /app/out

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef

# Copy the published app from the build stage
COPY --from=build /app/out .

# Copy migration script
COPY --from=build /src/migrate.sh .

# Make the script executable
RUN chmod +x migrate.sh

# Expose port 80
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run migrations and start the application
ENTRYPOINT ["/app/migrate.sh", "dotnet", "CharityPay.API.dll"]
