namespace HomeBook.Backend.EnvironmentHandler;

public static class PathHandler
{
    // the base-path is the parent path inside the docker container which should be mounted to the host system
#if MACOS
    // ReSharper disable once InconsistentNaming
    private static readonly string BASE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/homebook";
#else
    private const string BASE_PATH = "/var/lib/homebook";
#endif

    public static readonly string ConfigurationPath = Path.Combine(BASE_PATH, "config");
    public static readonly string RuntimeConfigurationFilePath = Path.Combine(ConfigurationPath, "homebook.appsettings.json");
    public static readonly string CacheDirectory = Path.Combine(BASE_PATH, "cache");
    public static readonly string LogDirectory = Path.Combine(BASE_PATH, "logs");
    public static readonly string DataDirectory = Path.Combine(BASE_PATH, "data");
    public static readonly string TempDirectory = Path.Combine(BASE_PATH, "temp");
    public static readonly string UpdateDirectory = Path.Combine(DataDirectory, "updates");
    public static readonly string StorageDirectory = Path.Combine(BASE_PATH, "storage");
}
