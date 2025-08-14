using HomeBook.Backend.Abstractions.Setup;

namespace HomeBook.Backend.Abstractions;

public interface ISetupConfigurationProvider
{
    string? GetValue(EnvironmentVariables name);
}
