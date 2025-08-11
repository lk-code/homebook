namespace Homebook.Backend.Core.Setup.Models;

/// <summary>
/// contains the environment configuration for the setup process.
/// </summary>
/// <param name="DatabaseHost">the hostname or IP address of the database server.</param>
/// <param name="DatabasePort">the port number on which the database server is listening.</param>
/// <param name="DatabaseName">the name of the database which should used by the application.</param>
/// <param name="DatabaseUserName">the username for accessing the database.</param>
/// <param name="DatabaseUserPassword">the password for the database user.</param>
public record EnvironmentConfiguration(string? DatabaseHost,
    string? DatabasePort,
    string? DatabaseName,
    string? DatabaseUserName,
    string? DatabaseUserPassword);
