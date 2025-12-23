namespace ECommerce.Tests.Unit.Helpers;

public class ExampleUnitTests
{
    [Fact]
    public void ExampleTest_ShouldPass()
    {
        // Arrange
        var value = 5;

        // Act
        var result = value * 2;

        // Assert
        result.Should().Be(10);
    }

    [Theory]
    [InlineData(2, 4)]
    [InlineData(3, 6)]
    [InlineData(5, 10)]
    public void ExampleTheoryTest_ShouldMultiplyCorrectly(int input, int expected)
    {
        // Arrange & Act
        var result = input * 2;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ExampleMockTest_ShouldVerifyCallCount()
    {
        // Arrange
        var mockService = new Mock<IExampleService>();
        mockService.Setup(s => s.GetData()).Returns("test data");

        // Act
        var result = mockService.Object.GetData();

        // Assert
        result.Should().Be("test data");
        mockService.Verify(s => s.GetData(), Times.Once);
    }
}

public interface IExampleService
{
    string GetData();
}
