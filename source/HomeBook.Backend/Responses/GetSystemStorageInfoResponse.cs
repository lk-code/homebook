namespace HomeBook.Backend.Responses;

/// <summary>
///
/// </summary>
/// <param name="Total"></param>
/// <param name="Used"></param>
/// <param name="Free"></param>
public record GetSystemStorageInfoResponse(long Total,
    long Used,
    long Free);
