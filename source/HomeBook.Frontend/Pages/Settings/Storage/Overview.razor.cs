using HomeBook.Frontend.Core.Models.Setup;
using HomeBook.Frontend.Core.Models.UserPreferences;
using HomeBook.Frontend.UI.Resources;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Pages.Settings.Storage;

public partial class Overview : ComponentBase
{
    private int _selectedStorageSegmentIndex = -1;
    private List<ChartSeries<double>> _storageUsageSeries = [];
    private string[] _storageUsageLabels = [];

    private MudForm _form = new();
    private readonly UserPreferenceLocalizationViewModel _configurationModel = new();
    private bool _isValid;
    private readonly List<LanguageViewModel> _availableLanguages = [];
    private bool _isLoading;

    protected override Task OnInitializedAsync()
    {
        // TODO: Hier die echten UsedSpace- und FreeSpace-Werte laden, sobald sie
        // im Backend-Response oder Provider verfuegbar sind, und an ReadStorageUsage(...) uebergeben.
        StorageUsageModel storageUsage = ReadStorageUsage();

        _storageUsageSeries =
        [
            new ChartSeries<double>
            {
                Name = "Storage",
                Data = new ChartData<double>(
                [
                    storageUsage.UsedSpace,
                    storageUsage.FreeSpace
                ])
            }
        ];

        _storageUsageLabels = CreateStorageUsageLabels(storageUsage);

        return Task.CompletedTask;
    }

    private static StorageUsageModel ReadStorageUsage(double? usedSpace = null,
        double? freeSpace = null)
    {
        double resolvedUsedSpace = usedSpace ?? 320d;
        double resolvedFreeSpace = freeSpace ?? 680d;

        return new StorageUsageModel(resolvedUsedSpace, resolvedFreeSpace);
    }

    private static string[] CreateStorageUsageLabels(StorageUsageModel storageUsage)
    {
        double totalSpace = storageUsage.UsedSpace + storageUsage.FreeSpace;

        if (totalSpace <= 0)
        {
            return ["Used", "Free"];
        }

        return
        [
            $"Used ({storageUsage.UsedSpace / totalSpace:P0})",
            $"Free ({storageUsage.FreeSpace / totalSpace:P0})"
        ];
    }

    private sealed record StorageUsageModel(double UsedSpace, double FreeSpace);

    private async Task UpdateInstanceConfigurationAsync()
    {
        if (!_isValid)
            return;

        try
        {
            _isLoading = true;

            // Validate form before proceeding
            await _form.Validate();
            if (!_form.IsValid)
                return;
        }
        catch (Exception err)
        {

        }
        finally
        {
            _isLoading = false;
        }
    }
}
