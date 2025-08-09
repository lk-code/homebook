namespace HomeBook.Backend.Responses;

public record GetDatabaseCheckResponse(string DatabaseName, string Username, string Password);
