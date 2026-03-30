using HomeBook.Frontend.Abstractions.Models;
using HomeBook.Frontend.UI.Resources;
using HomeBook.Frontend.ViewModels.Settings.Storage;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Pages.Settings.Storage;

public partial class Overview : ComponentBase
{
    private int _selectedStorageSegmentIndex = -1;
    private readonly List<ChartSeries<double>> _storageUsageSeries = [];
    private string[] _storageUsageLabels = [];
    private StorageUsageInformations? _storageUsageInformations = null;
    private readonly List<StorageEntryViewModel> _storageEntries = [];

    protected override async Task OnInitializedAsync()
    {
        // load storage usage
        _storageUsageInformations = await LoadStorageUsageAsync();
        StateHasChanged();

        // load data for list
        PrepareForStorageUsageList(_storageUsageInformations);
        StateHasChanged();

        // load data for chart
        PrepareForStorageUsageChart(_storageEntries);
        StateHasChanged();
    }

    private void PrepareForStorageUsageChart(List<StorageEntryViewModel> storageEntries)
    {
        _storageUsageSeries.Clear();

        List<double> storageUsages = [];
        List<string> storageUsageLabels = [];
        foreach (StorageEntryViewModel se in storageEntries)
        {
            storageUsageLabels.Add(se.Title);
            storageUsages.Add(se.UsageSizeBytes);
        }

        _storageUsageSeries.Add(new ChartSeries<double>
        {
            Name = Loc[nameof(LocalizationStrings.Settings_Storage_UsageChart_Usage_Title)],
            Data = new ChartData<double>(storageUsages)
        });

        _storageUsageLabels = storageUsageLabels.ToArray();
        StateHasChanged();
    }

    private string GetTranslation(string key) =>
        key.ToLowerInvariant() switch
        {
            "media" => Loc[nameof(LocalizationStrings.Settings_Storage_UsageType_Media_Title)],
            "cache" => Loc[nameof(LocalizationStrings.Settings_Storage_UsageType_Cache_Title)],
            "logs" => Loc[nameof(LocalizationStrings.Settings_Storage_UsageType_Logs_Title)],
            "temp" => Loc[nameof(LocalizationStrings.Settings_Storage_UsageType_Temp_Title)],
            _ => $"unknown translation key: {key}"
        };

    private void PrepareForStorageUsageList(StorageUsageInformations su)
    {
        _storageEntries.Clear();

        // add cache storage
        _storageEntries.Add(new StorageEntryViewModel(GetTranslation(su.CacheStorage.StorageKey),
            su.CacheStorage.UsageSizeBytes));

        // add logs storage
        _storageEntries.Add(new StorageEntryViewModel(GetTranslation(su.LogStorage.StorageKey),
            su.LogStorage.UsageSizeBytes));

        // add temp-data storage
        _storageEntries.Add(new StorageEntryViewModel(GetTranslation(su.TempDataStorage.StorageKey),
            su.TempDataStorage.UsageSizeBytes));

        // add media storage
        List<(string ModuleKey, string ModuleName)> modules = su.MediaStorage
            .Select(s => (s.ModuleKey, s.ModuleName))
            .Distinct()
            .ToList();
        long mediaTotalUsedSpace = 0;
        List<StorageEntryViewModel> mediaSubEntries = [];
        foreach ((string ModuleKey, string ModuleName) module in modules)
        {
            // module level
            long moduleSizeBytes = 0;
            List<StorageEntryViewModel> moduleSubEntries = [];

            // load all scopes for module
            MediaStorageUsageInformation[] moduleStorages = su.MediaStorage
                .Where(s => s.ModuleKey == module.ModuleKey)
                .ToArray();
            foreach (MediaStorageUsageInformation moduleStorage in moduleStorages)
            {
                moduleSizeBytes += moduleStorage.StorageSizeBytes;
                string translatedScope = Loc[moduleStorage.ScopeKey];
                moduleSubEntries.Add(new(translatedScope,
                    moduleStorage.StorageSizeBytes));
            }

            StorageEntryViewModel mediaSubEntry = new($"{module.ModuleName}",
                moduleSizeBytes,
                moduleSubEntries);
            mediaTotalUsedSpace += moduleSizeBytes;
            mediaSubEntries.Add(mediaSubEntry);
        }

        StorageEntryViewModel mediaStorageEntry = new(GetTranslation("media"),
            mediaTotalUsedSpace,
            mediaSubEntries);
        _storageEntries.Add(mediaStorageEntry);
    }

    private async Task<StorageUsageInformations> LoadStorageUsageAsync()
    {
        CancellationToken cancellationToken = CancellationToken.None;
        StorageUsageInformations storageUsage = await SystemStorageProvider.GetStorageUsageAsync(cancellationToken);

        return storageUsage;
    }
}
