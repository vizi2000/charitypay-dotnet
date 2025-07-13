using Xunit;

namespace CharityPay.Domain.Tests;

/// <summary>
/// Basic test to ensure the Domain project compiles and tests can run.
/// </summary>
public class DomainTestsSetup
{
    [Fact]
    public void Domain_Project_Should_Compile()
    {
        // Arrange & Act & Assert
        Assert.True(true, "Domain project compiles successfully");
    }

    [Fact]
    public void Domain_Tests_Should_Run()
    {
        // Arrange
        var testValue = "CharityPay Domain";
        
        // Act
        var result = testValue.Contains("Domain");
        
        // Assert
        Assert.True(result, "Domain tests are running correctly");
    }
}