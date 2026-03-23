using HomeBook.Frontend.Provider;
using Microsoft.Extensions.Configuration;

namespace HomeBook.UnitTests.Frontend.Provider;

[TestFixture]
public class AppUriProviderTests
{
    [Test]
    public void GetAbsoluteUri_WithSameServerConfig_Return()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {
                    "Backend:Host", "/app"
                },
                {
                    "Frontend:Host", "https://www.your-homebook.com/"
                }
            })
            .Build();
        var instance = new AppUriProvider(
            configuration);
        var relativeUri = "/images/test.jpg";

        // Act
        var absoluteUri = instance.GetAbsoluteUri(relativeUri);

        // Assert
        absoluteUri.ShouldNotBeNull();
        absoluteUri.ToString().ShouldBe("https://www.your-homebook.com/app/images/test.jpg");
    }

    [Test]
    public void GetAbsoluteUri_WithDifferentServerConfig_Return()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {
                    "Backend:Host", "https://backend.your-homebook.com/"
                },
                {
                    "Frontend:Host", "https://www.your-homebook.com/"
                }
            })
            .Build();
        var instance = new AppUriProvider(
            configuration);
        var relativeUri = "/images/test.jpg";

        // Act
        var absoluteUri = instance.GetAbsoluteUri(relativeUri);

        // Assert
        absoluteUri.ShouldNotBeNull();
        absoluteUri.ToString().ShouldBe("https://backend.your-homebook.com/images/test.jpg");
    }
}
