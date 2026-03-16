namespace HomeBook.Backend.Abstractions.Contracts;

public interface IFileSystemService
{
    bool FileExists(string path);
    Task<string> FileReadAllTextAsync(string path, CancellationToken cancellationToken);
    Task<byte[]> FileReadAllBytesAsync(string path, CancellationToken cancellationToken);
    Task FileWriteAllTextAsync(string path, string content, CancellationToken cancellationToken);
    Task FileWriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken);
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
    void DeleteFile(string path);
}
