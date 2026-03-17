using FluentDataBuilder;
using FluentDataBuilder.Microsoft.Extensions.Configuration;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Extensions;

namespace HomeBook.UnitTests.Backend.Extensions;

[TestFixture]
public class InstanceStatusExtensionsTests
{
    private string? _originalGitHubActionsValue;

    [SetUp]
    public void SetUp()
    {
        // Store the original value of GITHUB_ACTIONS environment variable
        _originalGitHubActionsValue = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");

        // Clear the environment variable for clean test state
        Environment.SetEnvironmentVariable("GITHUB_ACTIONS", null);
    }

    [TearDown]
    public void TearDown()
    {
        // Restore the original value of GITHUB_ACTIONS environment variable
        Environment.SetEnvironmentVariable("GITHUB_ACTIONS", _originalGitHubActionsValue);
    }

    [Test]
    public void ContainsUserAsync_WithEnvGitHubWorkflow_Return()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GITHUB_ACTIONS", "true");
        var configuration = new DataBuilder()
            .Add("Database",
                new DataBuilder()
                    .Add("Provider", (string?)null))
            .ToConfiguration();

        // Act
        var actual = configuration.GetCurrentInstanceStatus();

        // Assert
        actual.ShouldBe(InstanceStatus.RUNNING);
    }
}
