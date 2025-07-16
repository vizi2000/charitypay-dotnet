using CharityPay.Application.Services;
using CharityPay.Application.Tests.Fixtures;
using Moq;
using Xunit;

namespace CharityPay.Application.Tests.Services;

public class AuthenticationServiceTests : ApplicationTestBase
{
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests(ApplicationTestFixture fixture) : base(fixture)
    {
        _authService = ServiceTestHelper.CreateAuthenticationService(
            DbContext,
            passwordService: Mock.Of<IPasswordService>(),
            jwtService: _jwtServiceMock.Object,
            logger: CreateMockLogger<AuthenticationService>().Object);
    }

    [UnitTest]
    public async Task LogoutAsync_ShouldInvalidateRefreshToken()
    {
        // Arrange
        var refreshToken = "test-token";

        // Act
        await _authService.LogoutAsync(refreshToken);

        // Assert
        _jwtServiceMock.Verify(j => j.InvalidateRefreshToken(refreshToken), Times.Once);
    }
}
