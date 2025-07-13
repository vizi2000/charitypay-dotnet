using Xunit;

namespace CharityPay.Application.Tests;

/// <summary>
/// Basic test to ensure the Application project compiles and tests can run.
/// </summary>
public class ApplicationTestsSetup
{
    [Fact]
    public void Application_Project_Should_Compile()
    {
        // Arrange & Act & Assert
        Assert.True(true, "Application project compiles successfully");
    }

    [Fact]
    public void Application_Tests_Should_Run()
    {
        // Arrange
        var testValue = "CharityPay Application";
        
        // Act
        var result = testValue.Contains("Application");
        
        // Assert
        Assert.True(result, "Application tests are running correctly");
    }
}