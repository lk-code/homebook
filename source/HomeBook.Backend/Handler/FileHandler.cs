using Microsoft.AspNetCore.Mvc;
using HomeBook.Backend.Responses;
using HomeBook.Backend.Requests;

namespace HomeBook.Backend.Handler;

public static class FileHandler
{
    // GET - Read file content
    public static async Task<IResult> HandleGetFile([FromQuery] string filename, [FromQuery] Guid scopeId, CancellationToken cancellationToken)
    {
        // Empty handler - to be implemented
        return TypedResults.Ok(new FileGetResponse(filename, scopeId, Array.Empty<byte>()));
    }

    // POST - Create/Update file
    public static async Task<IResult> HandlePostFile([FromBody] FilePostRequest request, CancellationToken cancellationToken)
    {
        // Empty handler - to be implemented
        return TypedResults.Ok("File operation completed");
    }

    // DELETE - Delete file
    public static async Task<IResult> HandleDeleteFile([FromQuery] string filename, [FromQuery] Guid scopeId, CancellationToken cancellationToken)
    {
        // Empty handler - to be implemented
        return TypedResults.Ok("File deleted successfully");
    }
}
