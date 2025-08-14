using HomeBook.Backend.OpenApi;

namespace HomeBook.UnitTests.Backend.OpenApi;

[TestFixture]
public class DescriptionTests
{
    [Test]
    public void Description_With_Returns()
    {
        // Arrange
        var description = new Description("this is a description",
            "HTTP 200: this is a success response",
            "HTTP 400: this is a bad request response",
            "HTTP 500: this is an internal server error response");

        // Act
        var result = description.ToString();

        // Assert
        result.ShouldBe("this is a description"
                        + Environment.NewLine
                        + Environment.NewLine
                        + "HTTP 200: this is a success response" + Environment.NewLine
                        + "HTTP 400: this is a bad request response" + Environment.NewLine
                        + "HTTP 500: this is an internal server error response");
    }
}
