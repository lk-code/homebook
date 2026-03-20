using HomeBook.Backend.Abstractions.Models;

namespace HomeBook.Backend.Abstractions.Contracts;

public interface IFileSystemService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool FileExists(string path);

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> FileReadAllTextAsync(string path,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> FileReadAllBytesAsync(string path,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task FileWriteAllTextAsync(string path,
        string content,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task FileWriteAllBytesAsync(string path,
        byte[] content,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool DirectoryExists(string path);

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    DirectoryInfo CreateDirectory(string path);

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    void DeleteFile(string path);

    /// <summary>
    ///
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<FileInformation>> GetFilesInDirectoryAsync(string storagePath,
        CancellationToken cancellationToken);
}
