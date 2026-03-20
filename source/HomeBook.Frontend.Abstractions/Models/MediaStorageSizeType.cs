namespace HomeBook.Frontend.Abstractions.Models;

public record MediaStorageSizeType(
    string ScopeKey,
    string ModuleKey,
    string ModuleName,
    long StorageSizeBytes);
