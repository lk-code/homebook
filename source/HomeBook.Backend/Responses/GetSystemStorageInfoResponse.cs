namespace HomeBook.Backend.Responses;

/// <summary>
///
/// </summary>
/// <param name="Total"></param>
/// <param name="Used"></param>
/// <param name="Free"></param>
/// <param name="StorageByType"></param>
public record GetSystemStorageInfoResponse(long Total,
    long Used,
    long Free,
    MediaStorageSizeTypeResponse[] StorageByType);
