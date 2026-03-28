using Humanizer;

namespace HomeBook.Frontend.ViewModels.Settings.Storage;

public class StorageEntryViewModel(
    string title,
    long usageSizeBytes,
    List<StorageEntryViewModel> subEntries = null)
{
    public string Title { get; set; } = title;
    public long UsageSizeBytes { get; set; } = usageSizeBytes;
    public string UsageSizeFormatted => UsageSizeBytes.Bytes().Humanize();
    public List<StorageEntryViewModel> SubEntries { get; set; } = subEntries ?? [];
}
