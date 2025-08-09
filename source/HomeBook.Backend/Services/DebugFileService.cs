using HomeBook.Backend.Abstractions;

namespace HomeBook.Backend.Services;

public class DebugFileService : IFileService
{
    public async Task<bool> DoesFileExistsAsync(string path) => false;
}
