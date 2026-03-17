namespace HomeBook.Backend.Requests;

/// <summary>
/// Request model for creating/updating a file
/// </summary>
/// <param name="Filename">The filename (without path)</param>
/// <param name="Content">The binary content of the file</param>
/// <param name="ScopeId">The scope identifier</param>
public record FilePostRequest(string Filename,
    byte[] Content,
    Guid ScopeId);
