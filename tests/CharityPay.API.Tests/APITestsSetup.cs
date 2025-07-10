namespace CharityPay.API.Tests;

/// <summary>
/// Basic test to ensure the API project compiles and tests can run.
/// </summary>
public class APITestsSetup
{
    [Fact]
    public void API_Project_Should_Compile()
    {
        // Arrange & Act & Assert
        Assert.True(true, "API project compiles successfully");
    }

    [Fact]
    public void API_Tests_Should_Run()
    {
        // Arrange
        var testValue = "CharityPay API";
        
        // Act
        var result = testValue.Contains("API");
        
        // Assert
        Assert.True(result, "API tests are running correctly");
    }
}