using System.Reflection;
using AngleSharp;
using AngleSharp.Dom;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using Microsoft.Extensions.Logging;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace HomeBook.Backend.Core.Licenses;

public class LicenseProvider(
    ILogger<LicenseProvider> logger,
    IConfiguration configuration,
    IFileSystemService fileSystemService,
    IApplicationPathProvider applicationPathProvider) : ILicenseProvider
{
    /// <inheritdoc />
    public async Task<DependencyLicense[]> GetLicensesAsync(CancellationToken cancellationToken)
    {
        string executableLocation = Assembly.GetExecutingAssembly().Location;
        string executableDirectory = Path.GetDirectoryName(executableLocation) ?? string.Empty;
        string licensesDirectory = Path.Combine(executableDirectory, "Licenses");
        string[] filesInDirectory = Directory.GetFiles(licensesDirectory, "*.*", SearchOption.AllDirectories);

        List<DependencyLicense> licenses = [];
        foreach (string file in filesInDirectory)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileContent = await fileSystemService.FileReadAllTextAsync(file, cancellationToken);

            IBrowsingContext context = BrowsingContext.New(Configuration.Default);
            IDocument doc = await context.OpenAsync(r => r.Content(fileContent), cancellationToken);

            string bodyInnerHtml = doc.Body?.InnerHtml ?? fileContent;
            bodyInnerHtml = bodyInnerHtml.Trim()
                .Replace("id", "class");

            licenses.Add(new DependencyLicense(fileName, bodyInnerHtml));
        }



        // <a href="https://iradesign.io">Illustrations by IRA Design</a>

        return licenses.ToArray();
    }

    /// <inheritdoc />
    public async Task MarkLicenseAsAcceptedAsync(CancellationToken cancellationToken)
    {
        string instanceFilePath = Path.Combine(applicationPathProvider.DataDirectory, "licenses-accepted.txt");
        logger.LogInformation("Write licenses-accepted file at {FilePath}", instanceFilePath);

        string appVersion = configuration.GetSection("Version")?.Value?.Trim() ?? string.Empty;
        await fileSystemService.FileWriteAllTextAsync(instanceFilePath, appVersion, cancellationToken );
    }
}
