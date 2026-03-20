namespace HomeBook.Backend.Abstractions.Models;

public record StorageSize(
    long TotalSizeBytes,
    long UsedSizeBytes,
    long FreeSizeBytes);
