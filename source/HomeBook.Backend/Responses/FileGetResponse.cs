namespace HomeBook.Backend.Responses;

/// <summary>
/// Response model for file get operations
/// </summary>
/// <param name="Filename">The filename (without path)</param>
/// <param name="ScopeId">The scope identifier</param>
/// <param name="Content">The binary content of the file</param>
public record FileGetResponse(string Filename, Guid ScopeId, byte[] Content);