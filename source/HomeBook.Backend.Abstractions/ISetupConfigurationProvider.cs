using HomeBook.Backend.Abstractions.Setup;

namespace HomeBook.Backend.Abstractions;

public interface ISetupConfigurationProvider
{
    bool IsSet(EnvironmentVariables name);
    string? GetValue(EnvironmentVariables name);
}
