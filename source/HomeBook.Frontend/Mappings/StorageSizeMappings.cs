using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions;

namespace HomeBook.Frontend.Mappings;

public static class StorageSizeMappings
{
    public static StorageUsage ToDto(this StorageUsageResponse u)
    {
        return new StorageUsage(u.StorgeKey,
            u.UsageSizeBytes ?? 0);
    }
}
