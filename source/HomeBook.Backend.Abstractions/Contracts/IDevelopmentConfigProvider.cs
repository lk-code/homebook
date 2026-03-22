namespace HomeBook.Backend.Abstractions.Contracts;

public interface IDevelopmentConfigProvider
{
    Task<List<KeyValuePair<string, string?>>> GetConfigurationValuesAsync(CancellationToken cancellationToken);
}
