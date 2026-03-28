using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.ComponentModel.DataAnnotations;

namespace HomeBook.UnitTests.Backend.Handler;

[TestFixture]
public class AccountHandlerTests
{
    private IAccountProvider _accountProvider = null!;
    private ILogger<AccountHandler> _logger = null!;
    private IHttpContextAccessor _httpContextAccessor = null!;
    private HttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _accountProvider = Substitute.For<IAccountProvider>();
        _logger = Substitute.For<ILogger<AccountHandler>>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContext = Substitute.For<HttpContext>();

        _httpContextAccessor.HttpContext.Returns(_httpContext);
    }

    [Test]
    public async Task HandleLogin_ValidCredentials_ReturnsOkWithLoginResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var userId = Guid.NewGuid();
        var tokenResult = new JwtTokenResult
        {
            Token = "jwt-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UserId = userId,
            Username = "testuser"
        };

        _accountProvider.LoginAsync(request.Username, request.Password, Arg.Any<CancellationToken>())
            .Returns(tokenResult);

        // Act
        var result = await AccountHandler.HandleLogin(
            request,
            _accountProvider,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<LoginResponse>>();

        var okResult = result as Ok<LoginResponse>;
        okResult!.Value.ShouldNotBeNull();
        okResult.Value!.Token.ShouldBe("jwt-token");
        okResult.Value.RefreshToken.ShouldBe("refresh-token");
        okResult.Value.UserId.ShouldBe(userId);
        okResult.Value.Username.ShouldBe("testuser");

        await _accountProvider.Received(1).LoginAsync(request.Username, request.Password, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogin_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _accountProvider.LoginAsync(request.Username, request.Password, Arg.Any<CancellationToken>())
            .Returns((JwtTokenResult?)null);

        // Act
        var result = await AccountHandler.HandleLogin(
            request,
            _accountProvider,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<UnauthorizedHttpResult>();

        await _accountProvider.Received(1).LoginAsync(request.Username, request.Password, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogin_ValidationException_ReturnsBadRequestWithValidationProblemDetails()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var validationException = new ValidationException("Validation error");

        _accountProvider.LoginAsync(request.Username, request.Password, Arg.Any<CancellationToken>())
            .Throws(validationException);

        // Act
        var result = await AccountHandler.HandleLogin(
            request,
            _accountProvider,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<BadRequest<ValidationProblemDetails>>();

        var badRequestResult = result as BadRequest<ValidationProblemDetails>;
        badRequestResult!.Value.ShouldNotBeNull();
        badRequestResult.Value!.Title.ShouldBe("Validation Error");
        badRequestResult.Value.Detail.ShouldBe("Validation error");
        badRequestResult.Value.Status.ShouldBe(StatusCodes.Status400BadRequest);

        await _accountProvider.Received(1).LoginAsync(request.Username, request.Password, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogin_UnexpectedException_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var exception = new Exception("Unexpected error");

        _accountProvider.LoginAsync(request.Username, request.Password, Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        var result = await AccountHandler.HandleLogin(
            request,
            _accountProvider,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<UnauthorizedHttpResult>();

        await _accountProvider.Received(1).LoginAsync(request.Username, request.Password, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogout_ValidToken_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var token = "Bearer jwt-token";
        var httpRequest = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["Authorization"] = new StringValues(token)
        };

        httpRequest.Headers.Returns(headers);
        _httpContext.Request.Returns(httpRequest);

        _accountProvider.LogoutAsync("jwt-token", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await AccountHandler.HandleLogout(
            _accountProvider,
            _httpContextAccessor,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.Result.ShouldBeOfType<Ok<string>>();

        var okResult = result.Result as Ok<string>;
        okResult!.Value.ShouldBe("Logout successful");

        await _accountProvider.Received(1).LogoutAsync("jwt-token", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogout_LogoutFails_ReturnsBadRequest()
    {
        // Arrange
        var token = "Bearer invalid-token";
        var httpRequest = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["Authorization"] = new StringValues(token)
        };

        httpRequest.Headers.Returns(headers);
        _httpContext.Request.Returns(httpRequest);

        _accountProvider.LogoutAsync("invalid-token", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await AccountHandler.HandleLogout(
            _accountProvider,
            _httpContextAccessor,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.Result.ShouldBeOfType<BadRequest<string>>();

        var badRequestResult = result.Result as BadRequest<string>;
        badRequestResult!.Value.ShouldBe("Logout failed");

        await _accountProvider.Received(1).LogoutAsync("invalid-token", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogout_NullHttpContext_ReturnsBadRequest()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = await AccountHandler.HandleLogout(
            _accountProvider,
            _httpContextAccessor,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.Result.ShouldBeOfType<BadRequest<string>>();

        var badRequestResult = result.Result as BadRequest<string>;
        badRequestResult!.Value.ShouldBe("Invalid request context");

        await _accountProvider.DidNotReceive().LogoutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogout_NoAuthorizationHeader_ReturnsBadRequest()
    {
        // Arrange
        var httpRequest = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary();

        httpRequest.Headers.Returns(headers);
        _httpContext.Request.Returns(httpRequest);

        // Act
        var result = await AccountHandler.HandleLogout(
            _accountProvider,
            _httpContextAccessor,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.Result.ShouldBeOfType<BadRequest<string>>();

        var badRequestResult = result.Result as BadRequest<string>;
        badRequestResult!.Value.ShouldBe("No valid token provided");

        await _accountProvider.DidNotReceive().LogoutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogout_InvalidAuthorizationHeader_ReturnsBadRequest()
    {
        // Arrange
        var token = "InvalidToken";
        var httpRequest = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["Authorization"] = new StringValues(token)
        };

        httpRequest.Headers.Returns(headers);
        _httpContext.Request.Returns(httpRequest);

        // Act
        var result = await AccountHandler.HandleLogout(
            _accountProvider,
            _httpContextAccessor,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.Result.ShouldBeOfType<BadRequest<string>>();

        var badRequestResult = result.Result as BadRequest<string>;
        badRequestResult!.Value.ShouldBe("No valid token provided");

        await _accountProvider.DidNotReceive().LogoutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleLogout_ExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var token = "Bearer jwt-token";
        var httpRequest = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["Authorization"] = new StringValues(token)
        };

        httpRequest.Headers.Returns(headers);
        _httpContext.Request.Returns(httpRequest);

        var exception = new Exception("Unexpected error");
        _accountProvider.LogoutAsync("jwt-token", Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        var result = await AccountHandler.HandleLogout(
            _accountProvider,
            _httpContextAccessor,
            CancellationToken.None,
            _logger);

        // Assert
        result.ShouldNotBeNull();
        result.Result.ShouldBeOfType<BadRequest<string>>();

        var badRequestResult = result.Result as BadRequest<string>;
        badRequestResult!.Value.ShouldBe("Logout failed");

        await _accountProvider.Received(1).LogoutAsync("jwt-token", Arg.Any<CancellationToken>());
    }
}
