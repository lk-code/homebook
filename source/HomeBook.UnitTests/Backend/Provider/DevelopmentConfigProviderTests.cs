using HomeBook.Backend.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HomeBook.UnitTests.Backend.Provider;

[TestFixture]
public class DevelopmentConfigProviderTests
{
    private ILogger<DevelopmentConfigProvider> _logger;
    private IConfiguration _configuration = null!;
    private DevelopmentConfigProvider _instance = null!;

    [SetUp]
    public void SetUpSubstitutes()
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Debug);
        });

        _logger = factory.CreateLogger<DevelopmentConfigProvider>();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {
                    "Jwt:Issuer", "HomeBookTest"
                },
                {
                    "MustHave:SecretKey", "this-should-be-in-the-result"
                },
                {
                    "Jwt:SecretKey", "this-is-a-big-s3cr3t-key-with-a-lot-of-data-and-value"
                },
                {
                    "Jwt:Audience", "HomeBookTest"
                },
                {
                    "Counter", 54321.ToString()
                },
                {
                    "Database:Host", "localhost"
                },
                {
                    "Database:Password", "S3cr3tP1sSw0rD"
                },
                {
                    "Database:RandomPasswordActivated", true.ToString()
                },
            })
            .Build();
        _instance = new DevelopmentConfigProvider(_logger,
            _configuration);
    }

    [Test]
    public async Task GetConfigurationValuesAsync_CheckSensibleRemovalLogic_Return()
    {
        // Arrange

        // Act & Assert
        var configValues = await _instance.GetConfigurationValuesAsync(CancellationToken.None);
        configValues.ShouldNotBeNull();

        // Keys must still be present, but values must be masked
        configValues.ShouldContain(kvp => kvp.Key.Equals("Jwt:SecretKey", StringComparison.OrdinalIgnoreCase));
        configValues.ShouldContain(kvp => kvp.Key.Equals("Database:Password", StringComparison.OrdinalIgnoreCase));

        configValues.First(kvp => kvp.Key.Equals("Jwt:SecretKey", StringComparison.OrdinalIgnoreCase))
            .Value.ShouldBe("**********");
        configValues.First(kvp => kvp.Key.Equals("Database:Password", StringComparison.OrdinalIgnoreCase))
            .Value.ShouldBe("**********");

        // Non-sensitive keys must retain their original values
        configValues.First(kvp => kvp.Key.Equals("Jwt:Issuer", StringComparison.OrdinalIgnoreCase))
            .Value.ShouldBe("HomeBookTest");
        configValues.First(kvp => kvp.Key.Equals("MustHave:SecretKey", StringComparison.OrdinalIgnoreCase))
            .Value.ShouldBe("this-should-be-in-the-result");
        configValues.First(kvp => kvp.Key.Equals("Database:RandomPasswordActivated", StringComparison.OrdinalIgnoreCase))
            .Value.ShouldBe(true.ToString());
    }
}
