using HomeBook.Backend.Abstractions.Contracts;

namespace HomeBook.Backend.Provider;

public class DeveloperProvider(IConfiguration configuration) : IDeveloperProvider
{
    public async Task<bool> IsDevelopmentModeActiveAsync(CancellationToken cancellationToken)
    {
        bool isDevelopmentModeActive = configuration.GetValue<bool>("DevMode");

        await Task.CompletedTask;
        return isDevelopmentModeActive;
    }
}
