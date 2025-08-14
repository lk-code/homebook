namespace HomeBook.Backend.Requests;

public record CheckDatabaseRequest(string DatabaseHost,
    ushort DatabasePort,
    string DatabaseName,
    string DatabaseUserName,
    string DatabaseUserPassword);
