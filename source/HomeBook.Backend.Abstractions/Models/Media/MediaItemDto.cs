namespace HomeBook.Backend.Abstractions.Models.Media;

public record MediaItemDto(Guid Id,
    Guid ScopeId,
    string Filename);
