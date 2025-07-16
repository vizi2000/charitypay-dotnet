using System.Net;
using System.Net.Http.Json;
using CharityPay.Application.DTOs.Auth;
using CharityPay.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CharityPay.IntegrationTests;

public class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNewTokens_WhenValidRefreshTokenProvided()
    {
        // Arrange
        var registerRequest = new RegisterOrganizationRequest
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            OrganizationName = "Test Organization",
            Description = "Test Description",
            Category = OrganizationCategory.Dzieci,
            Location = "Test Location",
            TargetAmount = 10000,
            ContactEmail = "contact@example.com"
        };

        // Register and get initial tokens
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register-organization", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        
        var loginResponse = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.RefreshToken.Should().NotBeNullOrEmpty();

        // Act - Refresh the token
        var refreshRequest = new { refreshToken = loginResponse.RefreshToken };
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var newLoginResponse = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        newLoginResponse.Should().NotBeNull();
        newLoginResponse!.AccessToken.Should().NotBe(loginResponse.AccessToken);
        newLoginResponse.RefreshToken.Should().NotBe(loginResponse.RefreshToken);
        newLoginResponse.User.Email.Should().Be(registerRequest.Email);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenInvalidRefreshTokenProvided()
    {
        // Arrange
        var refreshRequest = new { refreshToken = "invalid-refresh-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenAlreadyUsed()
    {
        // Arrange
        var registerRequest = new RegisterOrganizationRequest
        {
            Email = "test2@example.com",
            Password = "Test123!@#",
            OrganizationName = "Test Organization 2",
            Description = "Test Description",
            Category = OrganizationCategory.Edukacja,
            Location = "Test Location",
            TargetAmount = 5000,
            ContactEmail = "contact2@example.com"
        };

        // Register and get initial tokens
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register-organization", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        
        var loginResponse = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var oldRefreshToken = loginResponse!.RefreshToken;

        // Use the refresh token once
        var refreshRequest = new { refreshToken = oldRefreshToken };
        var firstRefreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
        firstRefreshResponse.EnsureSuccessStatusCode();

        // Act - Try to use the same refresh token again
        var secondRefreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        secondRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldInvalidateRefreshToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@charitypay.pl",
            Password = "Admin123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        // Act - Logout
        var logoutRequest = new { refreshToken = tokens.RefreshToken };
        var logoutResponse = await _client.PostAsJsonAsync("/api/v1/auth/logout", logoutRequest);
        logoutResponse.EnsureSuccessStatusCode();

        // Try to use the refresh token after logout
        var refreshRequest = new { refreshToken = tokens.RefreshToken };
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}