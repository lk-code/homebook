using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Enums;
using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.EnvironmentHandler;

namespace HomeBook.Backend.Services;

public class NativeFileService(ILogger<NativeFileService> logger) : IApplicationPathProvider, IFileSystemService
{
    public string ConfigurationPath { get; } = PathHandler.ConfigurationPath;
    public string RuntimeConfigurationFilePath { get; } = PathHandler.RuntimeConfigurationFilePath;
    public string CacheDirectory { get; } = PathHandler.CacheDirectory;
    public string LogDirectory { get; } = PathHandler.LogDirectory;
    public string DataDirectory { get; } = PathHandler.DataDirectory;
    public string TempDirectory { get; } = PathHandler.TempDirectory;
    public string UpdateDirectory { get; } = PathHandler.UpdateDirectory;
    public string StorageDirectory { get; } = PathHandler.StorageDirectory;
    public string ExecutableDirectory { get; } = AppContext.BaseDirectory;

    /// <inheritdoc />
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <inheritdoc />
    public async Task<string> FileReadAllTextAsync(string path, CancellationToken cancellationToken)
    {
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<byte[]> FileReadAllBytesAsync(string path, CancellationToken cancellationToken)
    {
        return await File.ReadAllBytesAsync(path, cancellationToken);
    }

    /// <inheritdoc />
    public async Task FileWriteAllTextAsync(string path, string content, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    /// <inheritdoc />
    public async Task FileWriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken)
    {
        await File.WriteAllBytesAsync(path, content, cancellationToken);
    }

    /// <inheritdoc />
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <inheritdoc />
    public DirectoryInfo CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
    }

    /// <inheritdoc />
    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    /// <inheritdoc />
    public Task<List<FileInformation>> GetFilesInDirectoryAsync(string storagePath,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving files in directory");

        List<FileInformation> files = Directory
            .EnumerateFiles(storagePath, "*", SearchOption.AllDirectories)
            .Select(path =>
            {
                FileInfo info = new(path);
                return new FileInformation(info.FullName, info.Length);
            })
            .ToList();

        return Task.FromResult(files);
    }

    /// <inheritdoc />
    public Task<List<FileInformation>> GetAllInDirectoryAsync(string storagePath,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all entries in directory");

        List<FileInformation> entries = Directory
            .EnumerateFileSystemEntries(storagePath, "*", SearchOption.AllDirectories)
            .Select(path =>
            {
                bool isDirectory = Directory.Exists(path);
                long size = isDirectory ? 0 : new FileInfo(path).Length;
                return new FileInformation(path, size, isDirectory);
            })
            .ToList();

        return Task.FromResult(entries);
    }

    /// <inheritdoc />
    public string GetFolderPath(SpecialFolder folder)
    {
        return folder switch
        {
            SpecialFolder.MountedWallpaper => $"{DataDirectory}/wallpaper",
            SpecialFolder.ImageWallpaper => $"{ExecutableDirectory}/wallpaper",
            _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, null)
        };
    }

    /// <inheritdoc />
    public void CopyFile(string sourceFilePath,
        string targetFilePath,
        bool overwrite)
    {
        File.Copy(sourceFilePath, targetFilePath, overwrite);
    }
}
