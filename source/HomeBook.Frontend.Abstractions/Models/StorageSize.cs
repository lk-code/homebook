namespace HomeBook.Frontend.Abstractions.Models;

public record StorageSize(
    long TotalSizeBytes,
    long UsedSizeBytes,
    long FreeSizeBytes);
