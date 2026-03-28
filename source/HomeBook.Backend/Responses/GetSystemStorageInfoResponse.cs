namespace HomeBook.Backend.Responses;

/// <summary>
///
/// </summary>
/// <param name="Cache"></param>
/// <param name="Data"></param>
/// <param name="Logs"></param>
/// <param name="Storage"></param>
public record GetSystemStorageInfoResponse(StorageUsageResponse Cache,
    StorageUsageResponse Logs,
    StorageUsageResponse Temp,
    MediaStorageSizeTypeResponse[] MediaStorage);
