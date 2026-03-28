using HomeBook.Backend.Core.DataProvider;
using HomeBook.Backend.Core.HashProvider;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Validators;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HomeBook.UnitTests.Backend.Core.DataProvider;

[TestFixture]
public class UserProviderTests
{
    private CancellationToken _cancellationToken;
    private ILogger<UserProvider> _logger;
    private IUserRepository _userRepository;
    private UserProvider _instance;

    [SetUp]
    public void SetUp()
    {
        _cancellationToken = CancellationToken.None;
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
        _logger = factory.CreateLogger<UserProvider>();
        _userRepository = Substitute.For<IUserRepository>();
        _instance = new UserProvider(
            _userRepository,
            new HashProviderFactory(),
            new UserValidator(),
            _logger);
    }

    [Test]
    public async Task CreateUserAsync_WithValidUser_CallCreate()
    {
        // Arrange
        string username = "testuser";
        string password = "A-s3cr3t-P@ssw0rd!";
        Guid userId = Guid.NewGuid();
        _userRepository.ContainsUserAsync(username, _cancellationToken)
            .Returns(false);
        _userRepository.CreateUserAsync(Arg.Any<User>(), _cancellationToken)
            .Returns(new User
            {
                Id = userId,
                Username = username,
                PasswordHash = "passwordhash",
                PasswordHashType = "hash"
            });

        // Act
        Guid testUserId = await _instance.CreateUserAsync(username, password, _cancellationToken);

        // Assert
        testUserId.ShouldNotBe(Guid.Empty);
        await _userRepository.Received(1)
            .CreateUserAsync(
                Arg.Is<User>(u => u.Username == username &&
                                  !string.IsNullOrEmpty(u.PasswordHash) &&
                                  !string.IsNullOrEmpty(u.PasswordHashType)),
                _cancellationToken);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task ContainsUserAsync_ShouldReturnExpectedResult(bool containsUser)
    {
        // Arrange
        string username = "testuser";
        _userRepository.ContainsUserAsync(username, _cancellationToken)
            .Returns(containsUser);

        // Act
        var result = await _instance.ContainsUserAsync(username, _cancellationToken);

        // Assert
        result.ShouldBe(containsUser);
        await _userRepository.Received(1).ContainsUserAsync(username, _cancellationToken);
    }
}
