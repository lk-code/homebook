using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Services;

namespace HomeBook.UnitTests.TestCore.Backend.Services;

public class TestFileService : IApplicationPathProvider, IFileSystemService
{
    private static readonly IApplicationPathProvider NativeFileService = new NativeFileService();

    // Virtual file system storage
    private readonly Dictionary<string, string> _fileContents = new();
    private readonly HashSet<string> _directories = new();

    // Events for file system changes
    public event EventHandler<FileSystemEventArgs>? FileChanged;
    public event EventHandler<FileSystemEventArgs>? DirectoryCreated;

    public string ConfigurationPath { get; } = NativeFileService.ConfigurationPath;
    public string RuntimeConfigurationFilePath { get; } = NativeFileService.RuntimeConfigurationFilePath;
    public string CacheDirectory { get; } = NativeFileService.CacheDirectory;
    public string LogDirectory { get; } = NativeFileService.LogDirectory;
    public string DataDirectory { get; } = NativeFileService.DataDirectory;
    public string TempDirectory { get; } = NativeFileService.TempDirectory;
    public string UpdateDirectory { get; } = NativeFileService.UpdateDirectory;
    public string StorageDirectory { get; } = NativeFileService.StorageDirectory;

    public bool FileExists(string path)
    {
        return _fileContents.ContainsKey(NormalizePath(path));
    }

    public Task<string> FileReadAllTextAsync(string path, CancellationToken cancellationToken)
    {
        string normalizedPath = NormalizePath(path);

        return _fileContents.TryGetValue(normalizedPath, out string? content)
            ? Task.FromResult(content)
            : throw new FileNotFoundException($"File not found: {path}");
    }

    public Task FileWriteAllTextAsync(string path, string content, CancellationToken cancellationToken)
    {
        string normalizedPath = NormalizePath(path);

        // Ensure directory exists
        string? directoryPath = Path.GetDirectoryName(normalizedPath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            EnsureDirectoryExists(directoryPath);
        }

        _fileContents[normalizedPath] = content;

        // Trigger file changed event for both new and existing files
        FileChanged?.Invoke(this,
            new FileSystemEventArgs(normalizedPath,
                content,
                FileSystemEventType.FileChanged));

        return Task.CompletedTask;
    }

    public bool DirectoryExists(string path)
    {
        return _directories.Contains(NormalizePath(path));
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        string normalizedPath = NormalizePath(path);

        if (!_directories.Add(normalizedPath))
            return new DirectoryInfo(normalizedPath);

        DirectoryCreated?.Invoke(this,
            new FileSystemEventArgs(normalizedPath, FileSystemEventType.DirectoryCreated));

        // Return a mock DirectoryInfo
        return new DirectoryInfo(normalizedPath);
    }

    // Additional methods for managing the virtual file system

    /// <summary>
    /// Gets all files in the virtual file system
    /// </summary>
    public IReadOnlyDictionary<string, string> GetAllFiles()
    {
        return _fileContents.AsReadOnly();
    }

    /// <summary>
    /// Gets all directories in the virtual file system
    /// </summary>
    public IReadOnlyCollection<string> GetAllDirectories()
    {
        return _directories.ToList().AsReadOnly();
    }

    /// <summary>
    /// Clears all files and directories from the virtual file system
    /// </summary>
    public void ClearFileSystem()
    {
        _fileContents.Clear();
        _directories.Clear();
    }

    /// <summary>
    /// Adds a file to the virtual file system without triggering events
    /// </summary>
    public void SetFileContentSilently(string path, string content)
    {
        string normalizedPath = NormalizePath(path);
        string? directoryPath = Path.GetDirectoryName(normalizedPath);

        if (!string.IsNullOrEmpty(directoryPath))
            _directories.Add(NormalizePath(directoryPath));

        _fileContents[normalizedPath] = content;
    }

    /// <summary>
    /// Adds a directory to the virtual file system without triggering events
    /// </summary>
    public void AddDirectorySilently(string path)
    {
        _directories.Add(NormalizePath(path));
    }

    private void EnsureDirectoryExists(string directoryPath)
    {
        string normalizedPath = NormalizePath(directoryPath);

        if (!_directories.Add(normalizedPath))
            return;

        DirectoryCreated?.Invoke(this,
            new FileSystemEventArgs(normalizedPath,
                FileSystemEventType.DirectoryCreated));
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path).Replace('\\', '/');
    }
}

public class FileSystemEventArgs(
    string filePath,
    string? content,
    FileSystemEventType eventType) : EventArgs
{
    public FileSystemEventArgs(string filePath,
        FileSystemEventType eventType)
        : this(filePath, null, eventType)
    {
    }

    public string FilePath { get; } = filePath;
    public string? Content { get; } = content;
    public FileSystemEventType EventType { get; } = eventType;
}

public enum FileSystemEventType
{
    FileChanged,
    DirectoryCreated
}
