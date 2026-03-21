namespace HomeBook.Frontend.Abstractions.Models;

public record MediaStorageUsageInformation(
    string ScopeKey,
    string ModuleKey,
    string ModuleName,
    long StorageSizeBytes);
