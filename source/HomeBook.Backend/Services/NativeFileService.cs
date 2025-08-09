using HomeBook.Backend.Abstractions;

namespace HomeBook.Backend.Services;

public class NativeFileService(ILogger<NativeFileService> logger) : IFileService
{
    public async Task<bool> DoesFileExistsAsync(string path)
    {
        logger.LogInformation($"Checking if file exists: {path}");
        return File.Exists(path);
    }
}
