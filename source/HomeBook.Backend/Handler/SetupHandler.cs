using FluentValidation;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Exceptions;
using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Abstractions.Setup;
using Homebook.Backend.Core.Setup.Exceptions;
using Homebook.Backend.Core.Setup.Models;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public class SetupHandler
{
    /// <summary>
    /// checks if the setup is available and no setup instance is created yet.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="setupInstanceManager"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleGetAvailability([FromServices] ILogger<SetupHandler> logger,
        [FromServices] ISetupInstanceManager setupInstanceManager,
        CancellationToken cancellationToken)
    {
        try
        {
            // return TypedResults.Ok(); // HTTP 200
            // return TypedResults.Created(); // HTTP 201
            // return TypedResults.NoContent(); // HTTP 204
            // return TypedResults.InternalServerError(); // HTTP 500

            // 1. check the status of the setup and homebook instance
            bool setupFinished = setupInstanceManager.IsHomebookInstanceCreated();
            bool updateRequired = await setupInstanceManager.IsUpdateRequiredAsync(cancellationToken);

            // 2. LATER => check dependencies like Redis, etc.
            // to do later

            if (!setupFinished)
                // HTTP 200 => does not exist => setup is not executed yet and available
                return TypedResults.Ok();

            if (!updateRequired)
                // HTTP 204 => setup is finished and no update is required => Homebook is ready to use
                return TypedResults.NoContent();

            // HTTP 201 => update is required
            return TypedResults.Created();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving setup availability");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// returns all licenses of the project and if they are already accepted.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="licenseProvider"></param>
    /// <param name="setupConfigurationProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleGetLicenses([FromServices] ILogger<SetupHandler> logger,
        [FromServices] ILicenseProvider licenseProvider,
        [FromServices] ISetupConfigurationProvider setupConfigurationProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            string? licensesAreAcceptedValue =
                setupConfigurationProvider.GetValue(EnvironmentVariables.HOMEBOOK_CONFIGURATION_ACCEPT_LICENSES);
            DependencyLicense[] licenses = await licenseProvider.GetLicensesAsync(cancellationToken);
            GetLicensesResponse response = new(
                (licensesAreAcceptedValue is not null),
                licenses
            );

            return TypedResults.Ok(response);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving license information");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// returns the database configuration if it is available (set via environment variables).
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="setupConfigurationProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IResult HandleGetDatabaseCheck([FromServices] ILogger<SetupHandler> logger,
        [FromServices] ISetupConfigurationProvider setupConfigurationProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            string? databaseHost = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_HOST);
            string? databasePort = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_PORT);
            string? databaseName = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_NAME);
            string? databaseUserName = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_USER);
            string? databaseUserPassword = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_PASSWORD);

            bool databaseConfigurationFound = databaseHost is not null
                                              && databasePort is not null
                                              && databaseName is not null
                                              && databaseUserName is not null
                                              && databaseUserPassword is not null;
            if (!databaseConfigurationFound)
                return TypedResults.NotFound();

            GetDatabaseCheckResponse response = new(databaseHost,
                databasePort,
                databaseName,
                databaseUserName,
                databaseUserPassword);
            return TypedResults.Ok(response);
        }
        catch (ValidationException err)
        {
            logger.LogError(err, "Validation error while retrieving database configuration");
            string[] errors = err.Errors.Select(x => $"{x.PropertyName}, {x.ErrorMessage}").ToArray();
            return TypedResults.BadRequest(errors);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving database configuration");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// check the database connection via the given configuration.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="databaseProviderResolver"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleCheckDatabase([FromBody] CheckDatabaseRequest request,
        [FromServices] ILogger<SetupHandler> logger,
        [FromServices] IDatabaseProviderResolver databaseProviderResolver,
        CancellationToken cancellationToken)
    {
        try
        {
            string? resolvedDatabaseProvider = await databaseProviderResolver.ResolveAsync(
                request.DatabaseHost,
                request.DatabasePort,
                request.DatabaseName,
                request.DatabaseUserName,
                request.DatabaseUserPassword,
                cancellationToken);

            if (resolvedDatabaseProvider is not null)
                // database is available
                return TypedResults.Ok(resolvedDatabaseProvider.ToString()!.ToUpperInvariant());
            else
                // database is not available
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while checking database connection");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// check if a pre-configured user is available via environment variables
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="setupConfigurationProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IResult HandleGetPreConfiguredUser([FromServices] ILogger<SetupHandler> logger,
        [FromServices] ISetupConfigurationProvider setupConfigurationProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            string? homebookUserName = setupConfigurationProvider.GetValue(EnvironmentVariables.HOMEBOOK_USER_NAME);
            string? homebookUserPassword =
                setupConfigurationProvider.GetValue(EnvironmentVariables.HOMEBOOK_USER_PASSWORD);

            if (!string.IsNullOrEmpty(homebookUserName)
                && !string.IsNullOrEmpty(homebookUserPassword))
                return TypedResults.Ok();
            else
                return TypedResults.StatusCode(StatusCodes.Status404NotFound);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving pre-configured user");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// returns the homebook setup configuration if it is set via environment variables.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="setupConfigurationProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IResult HandleGetConfiguration([FromServices] ILogger<SetupHandler> logger,
        [FromServices] ISetupConfigurationProvider setupConfigurationProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            string? homebookInstanceName = setupConfigurationProvider
                .GetValue(EnvironmentVariables.HOMEBOOK_CONFIGURATION_NAME);

            if (!string.IsNullOrEmpty(homebookInstanceName))
                return TypedResults.Ok();
            else
                return TypedResults.NotFound();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving setup configuration");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// starts the homebook setup process.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="setupConfigurationValidator"></param>
    /// <param name="runtimeConfigurationProvider"></param>
    /// <param name="setupInstanceManager"></param>
    /// <param name="licenseProvider"></param>
    /// <param name="setupConfigurationProvider"></param>
    /// <param name="configuration"></param>
    /// <param name="hostApplicationLifetime"></param>
    /// <param name="setupProcessorFactory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleStartSetup([FromBody] StartSetupRequest request,
        [FromServices] ILogger<SetupHandler> logger,
        [FromServices] IValidator<SetupConfiguration> setupConfigurationValidator,
        [FromServices] IRuntimeConfigurationProvider runtimeConfigurationProvider,
        [FromServices] ISetupInstanceManager setupInstanceManager,
        [FromServices] ILicenseProvider licenseProvider,
        [FromServices] ISetupConfigurationProvider setupConfigurationProvider,
        [FromServices] IConfiguration configuration,
        [FromServices] IHostApplicationLifetime hostApplicationLifetime,
        [FromServices] ISetupProcessorFactory setupProcessorFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. map configuration (environment variables override request values)
            SetupConfiguration setupConfiguration = MapConfiguration(setupConfigurationProvider, request);
            await setupConfigurationValidator.ValidateAndThrowAsync(setupConfiguration, cancellationToken);

            // 2. create directory structure
            setupInstanceManager.CreateRequiredDirectories();

            // 3. mark the licenses of this version as accepted
            if (!setupConfiguration.HomebookAcceptLicenses)
                return TypedResults.StatusCode(StatusCodes.Status422UnprocessableEntity);

            await licenseProvider.MarkLicenseAsAcceptedAsync(cancellationToken);

            // 4. write database configuration to IConfuguration (runtime)
            await UpdateDatabaseConfigurationAsync(setupConfiguration,
                runtimeConfigurationProvider,
                cancellationToken);
            IConfigurationRoot root = (IConfigurationRoot)configuration;
            root.Reload();

            // 5. process primary setup
            ISetupProcessor setupProcessor = setupProcessorFactory.Create();
            await setupProcessor.ProcessAsync(configuration,
                setupConfiguration,
                cancellationToken);

            // 6. restart service
            hostApplicationLifetime.StopApplication();

            return TypedResults.Ok();
        }
        catch (UserAlreadyExistsException err)
        {
            logger.LogError(err, "Given user already exists");
            return TypedResults.BadRequest(err.Message);
        }
        catch (SetupException err)
        {
            logger.LogError(err, "Setup error while processing the setup");
            return TypedResults.BadRequest(err.Message);
        }
        catch (ValidationException err)
        {
            logger.LogError(err, "Validation error while starting setup");
            return TypedResults.BadRequest(err.Errors.Select(x => x.ErrorMessage).ToArray());
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while starting the setup");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    internal static SetupConfiguration MapConfiguration(ISetupConfigurationProvider scp,
        StartSetupRequest request)
    {
        SetupConfiguration defaultConfiguration = new()
        {
            DatabaseType = "UNKNOWN",
            HomebookConfigurationName = "",
            HomebookConfigurationDefaultLocale = "",
            HomebookUserName = "",
            HomebookUserPassword = "",
            HomebookAcceptLicenses = false
        };

        string? databaseType = request.DatabaseType
                               ?? scp.GetValue(EnvironmentVariables.DATABASE_TYPE);
        // DatabaseProvider databaseType = defaultConfiguration.DatabaseType;
        // if (!string.IsNullOrEmpty(databaseTypeValue))
        //     Enum.TryParse(databaseTypeValue, true, out databaseType);

        if (string.IsNullOrEmpty(databaseType))
            databaseType = "UNKNOWN";


        SetupConfiguration setupConfiguration = new()
        {
            DatabaseType = databaseType.ToUpperInvariant(),

            HomebookConfigurationName = request.HomebookConfigurationName
                                        ?? scp.GetValue(EnvironmentVariables.HOMEBOOK_CONFIGURATION_NAME)
                                        ?? defaultConfiguration.HomebookConfigurationName,

            HomebookConfigurationDefaultLocale = request.HomebookConfigurationDefaultLocale
                                                   ?? scp.GetValue(EnvironmentVariables.HOMEBOOK_CONFIGURATION_DEFAULT_LOCALE)
                                                   ?? defaultConfiguration.HomebookConfigurationDefaultLocale,

            HomebookUserName = request.HomebookUserName
                               ?? scp.GetValue(EnvironmentVariables.HOMEBOOK_USER_NAME)
                               ?? defaultConfiguration.HomebookUserName,

            HomebookUserPassword = request.HomebookUserPassword
                                   ?? scp.GetValue(EnvironmentVariables.HOMEBOOK_USER_PASSWORD)
                                   ?? defaultConfiguration.HomebookUserPassword,

            HomebookAcceptLicenses = request.LicensesAccepted ?? false
                || scp.GetValue(EnvironmentVariables.HOMEBOOK_CONFIGURATION_ACCEPT_LICENSES) is not null
                || defaultConfiguration.HomebookAcceptLicenses
        };

        if (setupConfiguration.DatabaseType == "SQLITE")
        {
            // add file settings
            setupConfiguration.DatabaseFile = request.DatabaseFile
                                              ?? scp.GetValue(EnvironmentVariables.DATABASE_FILE)
                                              ?? defaultConfiguration.DatabaseFile;
        }
        else
        {
            // add server settings
            setupConfiguration.DatabaseHost = request.DatabaseHost
                                              ?? scp.GetValue(EnvironmentVariables.DATABASE_HOST)
                                              ?? defaultConfiguration.DatabaseHost;
            setupConfiguration.DatabasePort = request.DatabasePort
                                              ?? (ushort?)scp.GetValue<ushort>(EnvironmentVariables.DATABASE_PORT)
                                              ?? defaultConfiguration.DatabasePort;
            setupConfiguration.DatabaseName = request.DatabaseName
                                              ?? scp.GetValue(EnvironmentVariables.DATABASE_NAME)
                                              ?? defaultConfiguration.DatabaseName;
            setupConfiguration.DatabaseUserName = request.DatabaseUserName
                                                  ?? scp.GetValue(EnvironmentVariables.DATABASE_USER)
                                                  ?? defaultConfiguration.DatabaseUserName;
            setupConfiguration.DatabaseUserPassword = request.DatabaseUserPassword
                                                      ?? scp.GetValue(EnvironmentVariables.DATABASE_PASSWORD)
                                                      ?? defaultConfiguration.DatabaseUserPassword;
        }

        return setupConfiguration;
    }

    private static async Task UpdateDatabaseConfigurationAsync(SetupConfiguration setupConfiguration,
        IRuntimeConfigurationProvider runtimeConfigurationProvider,
        CancellationToken cancellationToken)
    {
        await runtimeConfigurationProvider.UpdateConfigurationAsync("Database:Provider",
            setupConfiguration.DatabaseType,
            cancellationToken);

        switch (setupConfiguration.DatabaseType.ToUpperInvariant())
        {
            case "SQLITE":
            {
                await runtimeConfigurationProvider.UpdateConfigurationAsync("Database:FilePath",
                    setupConfiguration.DatabaseFile!,
                    cancellationToken);
            }
                break;
            case "MYSQL":
            case "POSTGRESQL":
            {
                await runtimeConfigurationProvider.UpdateConfigurationAsync("Database:Host",
                    setupConfiguration.DatabaseHost!,
                    cancellationToken);

                await runtimeConfigurationProvider.UpdateConfigurationAsync("Database:Port",
                    setupConfiguration.DatabasePort!,
                    cancellationToken);

                await runtimeConfigurationProvider.UpdateConfigurationAsync("Database:InstanceDbName",
                    setupConfiguration.DatabaseName!,
                    cancellationToken);

                await runtimeConfigurationProvider.UpdateConfigurationAsync("Database:Username",
                    setupConfiguration.DatabaseUserName!,
                    cancellationToken);

                // TODO: store password as encrypted value in .secret file
                await runtimeConfigurationProvider.UpdateConfigurationAsync("Database:Password",
                    setupConfiguration.DatabaseUserPassword!,
                    cancellationToken);
            }
                break;
            default:
                throw new NotSupportedException("Given database provider is not supported");
        }
    }
}
