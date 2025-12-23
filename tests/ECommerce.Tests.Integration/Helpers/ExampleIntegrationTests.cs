namespace ECommerce.Tests.Integration.Helpers;

public class ExampleIntegrationTests
{
    [Fact]
    public void ExampleIntegrationTest_ShouldPass()
    {
        // Arrange
        var testValue = "integration test";

        // Act
        var result = testValue.Length;

        // Assert
        result.Should().BeGreaterThan(0);
        testValue.Should().Contain("integration");
    }
}
