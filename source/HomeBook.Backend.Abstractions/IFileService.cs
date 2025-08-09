namespace HomeBook.Backend.Abstractions;

public interface IFileService
{
    Task<bool> DoesFileExistsAsync(string path);
}
