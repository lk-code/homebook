using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Mappings;

public static class StorageUsageMappings
{
    public static StorageUsageResponse ToResponse(this StorageUsage s) =>
        new StorageUsageResponse(s.StorgeKey,
            s.UsageSizeBytes);
}
