namespace HomeBook.Backend.Responses;

/// <summary>
/// contains the database configuration information from the environment variables.
/// </summary>
/// <param name="DatabaseHost">the database host</param>
/// <param name="DatabasePort">the database port</param>
/// <param name="DatabaseName">the database name</param>
/// <param name="DatabaseUserName">the database user name</param>
/// <param name="DatabaseUserPassword">the database user password</param>
public record GetDatabaseCheckResponse(string? DatabaseHost,
    string? DatabasePort,
    string? DatabaseName,
    string? DatabaseUserName,
    string? DatabaseUserPassword);
