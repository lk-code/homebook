namespace HomeBook.Backend.Abstractions.Contracts;

public interface IApplicationPathProvider
{
    string ConfigurationPath { get; }
    string RuntimeConfigurationFilePath { get; }
    string CacheDirectory { get; }
    string LogDirectory { get; }
    string DataDirectory { get; }
    string TempDirectory { get; }
    string UpdateDirectory { get; }
    string StorageDirectory { get; }
    string ExecutableDirectory { get; }
}
