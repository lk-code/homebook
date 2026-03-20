using HomeBook.Frontend.Abstractions.Models;
using Humanizer;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Pages.Settings.Storage;

public partial class Overview : ComponentBase
{
    private int _selectedStorageSegmentIndex = -1;
    private List<ChartSeries<double>> _storageUsageSeries = [];
    private string[] _storageUsageLabels = [];
    private StorageUsageModel? _storageUsage = null;

    protected override async Task OnInitializedAsync()
    {
        StorageUsageModel storageUsage = await ReadStorageUsage();
        _storageUsage = storageUsage;
        StateHasChanged();

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
        StateHasChanged();
    }

    private async Task<StorageUsageModel> ReadStorageUsage()
    {
        CancellationToken cancellationToken = CancellationToken.None;
        StorageSize storageUsage = await SystemStorageProvider.GetStorageUsageAsync(cancellationToken);

        long totalSpaceBytes = storageUsage.TotalSizeBytes;
        long usedSpaceBytes = storageUsage.UsedSizeBytes;
        long freeSpaceBytes = storageUsage.FreeSizeBytes;

        return new StorageUsageModel(totalSpaceBytes, usedSpaceBytes, freeSpaceBytes);
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
            $"Used ({storageUsage.UsedSpace.Bytes().Humanize()})",
            $"Free ({storageUsage.FreeSpace.Bytes().Humanize()})"
        ];
    }

    private sealed record StorageUsageModel(
        long CompleteSpace,
        long UsedSpace,
        long FreeSpace);
}
