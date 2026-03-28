namespace HomeBook.Backend.Module.Kitchen.Exceptions;

public class RecipeNotFoundException(string message,
    Exception err)
    : Exception(message,
        err);
