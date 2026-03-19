namespace HomeBook.Frontend.Module.Kitchen.ViewModels;

public sealed class MediaItemViewModel
{
    public MediaItemViewModel(Guid id,
        Uri absoluteUri,
        int index)
    {
        Id = id;
        AbsoluteUri = absoluteUri;
        Index = index;
    }

    public Guid Id { get; set; }

    public Uri AbsoluteUri { get; set; }

    public int Index { get; set; }
}
