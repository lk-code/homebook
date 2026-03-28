namespace HomeBook.Backend.Responses;

public record StorageUsageResponse(string StorgeKey,
    long UsageSizeBytes);
