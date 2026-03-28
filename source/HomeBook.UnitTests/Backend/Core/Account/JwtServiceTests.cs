using HomeBook.Backend.Core.Account;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;

namespace HomeBook.UnitTests.Backend.Core.Account;

[TestFixture]
public class JwtServiceTests
{
    private IConfiguration _configuration = null!;
    private JwtService _instance = null!;

    [SetUp]
    public void SetUp()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {
                    "Jwt:SecretKey", "this-is-a-big-s3cr3t-key-with-a-lot-of-data-and-value"
                },
                {
                    "Jwt:Issuer", "HomeBookTest"
                },
                {
                    "Jwt:Audience", "HomeBookTest"
                },
                {
                    "Jwt:ExpirationMinutes", "120"
                }
            })
            .Build();
        _instance = new JwtService(_configuration,
            NullLogger<JwtService>.Instance);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GenerateToken_AndVerify_Return(bool withAdmin)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act & Assert
        var jwt = _instance.GenerateToken(userId, username, withAdmin);
        jwt.ShouldNotBeNull();

        var token = jwt.Token;
        token.ShouldNotBeNullOrEmpty();

        var isValidToken = _instance.ValidateToken(token);
        isValidToken.ShouldBeTrue();

        var hasAdminFlag = _instance.IsAdminFromToken(token);
        hasAdminFlag.ShouldBe(withAdmin);

        var userIdFromToken = _instance.GetUserIdFromToken(token);
        userIdFromToken.ShouldBe(userId);
    }

    [Test]
    public void GenerateTokenWithOutAdminFlag_Return()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act & Assert
        var jwt = _instance.GenerateToken(userId, username);
        jwt.ShouldNotBeNull();

        var token = jwt.Token;
        token.ShouldNotBeNullOrEmpty();

        var isValidToken = _instance.ValidateToken(token);
        isValidToken.ShouldBeTrue();

        var hasAdminFlag = _instance.IsAdminFromToken(token);
        hasAdminFlag.ShouldBeFalse();

        var userIdFromToken = _instance.GetUserIdFromToken(token);
        userIdFromToken.ShouldBe(userId);
    }

    [Test]
    public void GenerateTokenWithInvalidToken_ThrowsAndReturns()
    {
        // Arrange

        // Act & Assert
        var token = "this-is-an-invalid-token";
        token.ShouldNotBeNullOrEmpty();

        var isValidToken = _instance.ValidateToken(token);
        isValidToken.ShouldBeFalse();

        var hasAdminFlag = _instance.IsAdminFromToken(token);
        hasAdminFlag.ShouldBeFalse();

        var userIdFromToken = _instance.GetUserIdFromToken(token);
        userIdFromToken.ShouldBeNull();
    }
}
