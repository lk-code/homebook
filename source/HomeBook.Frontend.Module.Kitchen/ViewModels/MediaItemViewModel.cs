namespace HomeBook.Frontend.Module.Kitchen.ViewModels;

public sealed class MediaItemViewModel(
    Guid id,
    Uri? absoluteUri,
    int index)
{
    public Guid Id { get; set; } = id;

    public Uri? AbsoluteUri { get; set; } = absoluteUri;

    public int Index { get; set; } = index;
}
