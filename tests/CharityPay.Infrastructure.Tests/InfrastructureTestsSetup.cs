using Xunit;

namespace CharityPay.Infrastructure.Tests;

/// <summary>
/// Basic test to ensure the Infrastructure project compiles and tests can run.
/// </summary>
public class InfrastructureTestsSetup
{
    [Fact]
    public void Infrastructure_Project_Should_Compile()
    {
        // Arrange & Act & Assert
        Assert.True(true, "Infrastructure project compiles successfully");
    }

    [Fact]
    public void Infrastructure_Tests_Should_Run()
    {
        // Arrange
        var testValue = "CharityPay Infrastructure";
        
        // Act
        var result = testValue.Contains("Infrastructure");
        
        // Assert
        Assert.True(result, "Infrastructure tests are running correctly");
    }
}