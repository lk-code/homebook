namespace HomeBook.Backend.Module.Kitchen.Requests;

public record CreateRecipeMediaItemRequest(
    Guid MediaItemId,
    int Index);
