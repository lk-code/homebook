namespace HomeBook.Backend.Responses;

public record MediaStorageSizeTypeResponse(
    string ScopeKey,
    string ModuleKey,
    string ModuleName,
    long StorageSizeBytes);
