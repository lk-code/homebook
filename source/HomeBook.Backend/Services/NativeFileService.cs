using HomeBook.Backend.Abstractions;

namespace HomeBook.Backend.Services;

public class NativeFileService : IFileService
{
    public async Task<bool> DoesFileExistsAsync(string path)
    {
        return File.Exists(path);
    }
}
