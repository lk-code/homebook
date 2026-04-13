using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Services;
using HomeBook.UnitTests.TestCore.Frontend;
using HomeBook.UnitTests.TestCore.Helper;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HomeBook.UnitTests.Frontend.Services;

[TestFixture]
public class AuthenticationServiceTests
{
    private ILoggerFactory _loggerFactory;
    private IRequestAdapter _backendClientAdapter;
    private BackendClient _backendClient;
    private TestJSRuntime _jsRuntime;
    private AuthenticationService _instance;

    [SetUp]
    public void SetUp()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Debug);
        });
        _backendClientAdapter = Substitute.For<IRequestAdapter>();
        _backendClient = new BackendClient(_backendClientAdapter);
        _jsRuntime = new TestJSRuntime();
        _instance = new AuthenticationService(
            _loggerFactory.CreateLogger<AuthenticationService>(),
            _backendClient,
            _jsRuntime);
    }

    [TearDown]
    public void TearDown()
    {
        _loggerFactory.Dispose();
    }

    [Test]
    public async Task LoginAsync_WithData_Return()
    {
        // arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var password = "s3cr3t";
        var cancellationToken = CancellationToken.None;
        var tokenResponse = "a-s-e-c-r-e-t-t-o-k-e-n";
        var refreshTokenResponse = "a-s-e-c-r-e-t-r-e-f-r-e-s-h-t-o-k-e-n";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", tokenResponse, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", expectedExpiresAt, "expiresAt");

        RequestInformation? postRequest = null;
        _backendClientAdapter.SendAsync(
                Arg.Do<RequestInformation>(req => postRequest = req),
                Arg.Any<ParsableFactory<LoginResponse>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new LoginResponse()
            {
                Token = tokenResponse,
                RefreshToken = refreshTokenResponse,
                ExpiresAt = expiresAt,
                UserId = userId,
                Username = username
            });
        var eventTriggered = false;
        _instance.AuthenticationStateChanged += async (isAuthenticated, token) =>
        {
            isAuthenticated.ShouldBeTrue();
            eventTriggered = true;
        };

        // act
        var result = await _instance.LoginAsync(username,
            password,
            cancellationToken);

        // assert
        _jsRuntime.Called("localStorage.setItem", "authToken", tokenResponse).ShouldBeTrue();
        _jsRuntime.Called("localStorage.setItem", "refreshToken", refreshTokenResponse).ShouldBeTrue();
        _jsRuntime.Called("localStorage.setItem", "expiresAt", expectedExpiresAt).ShouldBeTrue();
        eventTriggered.ShouldBeTrue();

        result.ShouldBeTrue();

        var isAuthenticated = await _instance.IsAuthenticatedAsync(cancellationToken);
        isAuthenticated.ShouldBeTrue();
    }

    [Test]
    public async Task LoginAsync_WithNoResponseFromServer_Return()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var password = "s3cr3t";
        var cancellationToken = CancellationToken.None;
        var tokenResponse = "a-s-e-c-r-e-t-t-o-k-e-n";
        var refreshTokenResponse = "a-s-e-c-r-e-t-r-e-f-r-e-s-h-t-o-k-e-n";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        RequestInformation? postRequest = null;
        _backendClientAdapter.SendAsync(
                Arg.Do<RequestInformation>(req => postRequest = req),
                Arg.Any<ParsableFactory<LoginResponse>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>())
            .Returns((LoginResponse)null!);
        var eventTriggered = false;
        _instance.AuthenticationStateChanged += async (isAuthenticated, token) =>
        {
            isAuthenticated.ShouldBeTrue();
            eventTriggered = true;
        };
        var result = await _instance.LoginAsync(username,
            password,
            cancellationToken);

        _jsRuntime.Called("localStorage.setItem", "authToken", tokenResponse).ShouldBeFalse();
        _jsRuntime.Called("localStorage.setItem", "refreshToken", refreshTokenResponse).ShouldBeFalse();
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");
        _jsRuntime.Called("localStorage.setItem", "expiresAt", expectedExpiresAt).ShouldBeFalse();
        eventTriggered.ShouldBeFalse();

        result.ShouldBeFalse();
    }

    [Test]
    public async Task LoginAsync_WithHttp400AsInvalidAuthFromServer_Return()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var password = "s3cr3t";
        var cancellationToken = CancellationToken.None;
        var tokenResponse = "a-s-e-c-r-e-t-t-o-k-e-n";
        var refreshTokenResponse = "a-s-e-c-r-e-t-r-e-f-r-e-s-h-t-o-k-e-n";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        RequestInformation? postRequest = null;
        _backendClientAdapter.SendAsync(
                Arg.Do<RequestInformation>(req => postRequest = req),
                Arg.Any<ParsableFactory<LoginResponse>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new ApiException("Bad Request").WithStatusCode(400));
        var eventTriggered = false;
        _instance.AuthenticationStateChanged += async (isAuthenticated, token) =>
        {
            isAuthenticated.ShouldBeTrue();
            eventTriggered = true;
        };
        var result = await _instance.LoginAsync(username,
            password,
            cancellationToken);

        _jsRuntime.Called("localStorage.setItem", "authToken", tokenResponse).ShouldBeFalse();
        _jsRuntime.Called("localStorage.setItem", "refreshToken", refreshTokenResponse).ShouldBeFalse();
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");
        _jsRuntime.Called("localStorage.setItem", "expiresAt", expectedExpiresAt).ShouldBeFalse();
        eventTriggered.ShouldBeFalse();

        result.ShouldBeFalse();
    }

    [Test]
    public async Task LoginAsync_WithHttp401AsInvalidAuthFromServer_Return()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var password = "s3cr3t";
        var cancellationToken = CancellationToken.None;
        var tokenResponse = "a-s-e-c-r-e-t-t-o-k-e-n";
        var refreshTokenResponse = "a-s-e-c-r-e-t-r-e-f-r-e-s-h-t-o-k-e-n";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        RequestInformation? postRequest = null;
        _backendClientAdapter.SendAsync(
                Arg.Do<RequestInformation>(req => postRequest = req),
                Arg.Any<ParsableFactory<LoginResponse>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new ApiException("Unauthorized").WithStatusCode(401));
        var eventTriggered = false;
        _instance.AuthenticationStateChanged += async (isAuthenticated, token) =>
        {
            isAuthenticated.ShouldBeTrue();
            eventTriggered = true;
        };
        var result = await _instance.LoginAsync(username,
            password,
            cancellationToken);

        _jsRuntime.Called("localStorage.setItem", "authToken", tokenResponse).ShouldBeFalse();
        _jsRuntime.Called("localStorage.setItem", "refreshToken", refreshTokenResponse).ShouldBeFalse();
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");
        _jsRuntime.Called("localStorage.setItem", "expiresAt", expectedExpiresAt).ShouldBeFalse();
        eventTriggered.ShouldBeFalse();

        result.ShouldBeFalse();
    }

    [Test]
    public async Task LoginAsync_WithHttp500AsInvalidAuthFromServer_Return()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var password = "s3cr3t";
        var cancellationToken = CancellationToken.None;
        var tokenResponse = "a-s-e-c-r-e-t-t-o-k-e-n";
        var refreshTokenResponse = "a-s-e-c-r-e-t-r-e-f-r-e-s-h-t-o-k-e-n";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        RequestInformation? postRequest = null;
        _backendClientAdapter.SendAsync(
                Arg.Do<RequestInformation>(req => postRequest = req),
                Arg.Any<ParsableFactory<LoginResponse>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new ApiException("Internal Server Error").WithStatusCode(500));
        var eventTriggered = false;
        _instance.AuthenticationStateChanged += async (isAuthenticated, token) =>
        {
            isAuthenticated.ShouldBeTrue();
            eventTriggered = true;
        };
        var result = await _instance.LoginAsync(username,
            password,
            cancellationToken);

        _jsRuntime.Called("localStorage.setItem", "authToken", tokenResponse).ShouldBeFalse();
        _jsRuntime.Called("localStorage.setItem", "refreshToken", refreshTokenResponse).ShouldBeFalse();
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");
        _jsRuntime.Called("localStorage.setItem", "expiresAt", expectedExpiresAt).ShouldBeFalse();
        eventTriggered.ShouldBeFalse();

        result.ShouldBeFalse();
    }

    [Test]
    public async Task IsAuthenticatedAsync_WithoutTokenStored_Return()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", (string)null!, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", expectedExpiresAt, "expiresAt");

        // act
        var isAuthenticated = await _instance.IsAuthenticatedAsync(cancellationToken);

        // assert
        isAuthenticated.ShouldBeFalse();
    }

    [Test]
    public async Task IsAuthenticatedAsync_WithoutExpiredStored_Return()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var tokenResponse = "a-s-e-c-r-e-t-t-o-k-e-n";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", tokenResponse, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", (string)null!, "expiresAt");

        // act
        var isAuthenticated = await _instance.IsAuthenticatedAsync(cancellationToken);

        // assert
        isAuthenticated.ShouldBeFalse();
    }

    [Test]
    public async Task IsAuthenticatedAsync_WithExpiredToken_Return()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var tokenResponse = "a-s-e-c-r-e-t-t-o-k-e-n";
        var expiresAt = DateTime.UtcNow.AddHours(-1);
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", tokenResponse, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", expectedExpiresAt, "expiresAt");

        // act
        var eventTriggered = false;
        _instance.AuthenticationStateChanged += async (isAuthenticated, token) =>
        {
            isAuthenticated.ShouldBeFalse();
            eventTriggered = true;
        };
        var isAuthenticated = await _instance.IsAuthenticatedAsync(cancellationToken);

        // assert
        isAuthenticated.ShouldBeFalse();
        _jsRuntime.Called("localStorage.removeItem", "authToken").ShouldBeTrue();
        _jsRuntime.Called("localStorage.removeItem", "refreshToken").ShouldBeTrue();
        _jsRuntime.Called("localStorage.removeItem", "expiresAt").ShouldBeTrue();
        eventTriggered.ShouldBeTrue();
    }

    [Test]
    public async Task IsAuthenticatedAsync_WithInvalidExpired_Return()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var tokenResponse = "a-s-e-c-r-e-t-t-o-k-e-n";
        var expiresAt = DateTime.UtcNow.AddHours(-1);
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", tokenResponse, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", "invalid-expired-datetime", "expiresAt");

        // act
        var isAuthenticated = await _instance.IsAuthenticatedAsync(cancellationToken);

        // assert
        isAuthenticated.ShouldBeFalse();
    }

    [Test]
    public async Task IsAdminOrThrowAsync_WithValidUserAsAdmin_Return()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var userId = Guid.NewGuid();
        var username = "testuser";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var testToken = JwtTokenHelper.GenerateToken(userId, username, expiresAt, true);
        var tokenResponse = testToken.Token;
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", tokenResponse, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", expectedExpiresAt, "expiresAt");

        // act & assert
        await Should.NotThrowAsync(async () => await _instance.IsAdminOrThrowAsync(CancellationToken.None));
    }

    [Test]
    public async Task IsAdminOrThrowAsync_WithValidUserAsNonAdmin_Throws()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var userId = Guid.NewGuid();
        var username = "testuser";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var testToken = JwtTokenHelper.GenerateToken(userId, username, expiresAt, false);
        var tokenResponse = testToken.Token;
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", tokenResponse, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", expectedExpiresAt, "expiresAt");

        // act & assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            await _instance.IsAdminOrThrowAsync(CancellationToken.None));

        exception.Message.ShouldBe("User is not authorized to access system information.");
    }

    [Test]
    public async Task IsAdminOrThrowAsync_WithoutAdminFlagInToken_Throws()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var userId = Guid.NewGuid();
        var username = "testuser";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var testToken = JwtTokenHelper.GenerateToken(userId, username, expiresAt, false, true);
        var tokenResponse = testToken.Token;
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", tokenResponse, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", expectedExpiresAt, "expiresAt");

        // act & assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            await _instance.IsAdminOrThrowAsync(CancellationToken.None));

        exception.Message.ShouldBe("User is not authorized to access system information.");
    }

    [Test]
    public async Task IsAdminOrThrowAsync_WithInvalidToken_Throws()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var tokenResponse = "invalid-token";
        var expectedExpiresAt = new DateTimeOffset(expiresAt, TimeSpan.FromHours(0)).DateTime.ToString("O");

        _jsRuntime.SetupResult("localStorage.getItem", tokenResponse, "authToken");
        _jsRuntime.SetupResult("localStorage.getItem", expectedExpiresAt, "expiresAt");

        // act & assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            await _instance.IsAdminOrThrowAsync(CancellationToken.None));

        exception.Message.ShouldBe("User is not authorized to access system information.");
    }
}
