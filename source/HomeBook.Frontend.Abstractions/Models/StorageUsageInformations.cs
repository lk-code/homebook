namespace HomeBook.Frontend.Abstractions.Models;

public record StorageUsageInformations(
    StorageUsage CacheStorage,
    StorageUsage LogStorage,
    StorageUsage TempDataStorage,
    MediaStorageUsageInformation[] MediaStorage);
