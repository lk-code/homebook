using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Models;
using HomeBook.Frontend.Modules.Abstractions;
using Humanizer;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Components;

public partial class UiSearchComponent : ComponentBase, IDisposable
{
    private static readonly DialogOptions SearchDialogOptions = new()
    {
        Position = DialogPosition.TopCenter,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
        BackdropClick = true,
        CloseOnEscapeKey = true,
        NoHeader = true,
        CloseButton = false
    };

    private long _searchSequence;
    private bool _disposed;
    private HashSet<Type> _availableSearchHandlerResultTemplateTypes = [];

    private bool IsSearching { get; set; }
    private string? SearchText { get; set; }
    private IReadOnlyList<SearchModuleGroup> SearchResults { get; set; } = [];
    private bool ShowFlyout => IsSearching || SearchResults.Count > 0;

    private string RootCssClass
    {
        get
        {
            List<string> classes = ["ui-search-component"];

            if (ShowFlyout)
                classes.Add("is-open");

            if (!string.IsNullOrWhiteSpace(Class))
                classes.Add(Class);

            return string.Join(" ", classes);
        }
    }

    [Inject]
    private BackendClient BackendClient { get; set; } = null!;

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private ILogger<UiSearchComponent> Logger { get; set; } = null!;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public int DebounceDelayMs { get; set; } = 3000;

    protected override void OnInitialized()
    {
        _availableSearchHandlerResultTemplateTypes = SearchHandlerResultTemplates
            .Select(template => template.GetType())
            .ToHashSet();
    }

    private async Task OnSearchTextChangedAsync(string? value)
    {
        SearchText = value;
        ResetSearchState();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        long currentSequence = _searchSequence;

        await InvokeAsync(StateHasChanged);

        await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(0, DebounceDelayMs)));
        if (IsSearchRequestOutdated(currentSequence))
            return;

        IsSearching = true;
        await InvokeAsync(StateHasChanged);

        await SearchAsync(currentSequence);
    }

    private void CloseFlyout()
    {
        ResetSearchState();
    }

    private Task OnDialogVisibleChanged(bool visible)
    {
        if (!visible)
            CloseFlyout();

        return Task.CompletedTask;
    }

    private async Task SearchAsync(long currentSequence)
    {
        try
        {
            string query = SearchText?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(query))
            {
                SearchResults = [];
                return;
            }

            string? token = await AuthenticationService.GetTokenAsync();
            if (IsSearchRequestOutdated(currentSequence))
                return;

            SearchResponse? response = await BackendClient.Search.GetAsync(x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                    x.QueryParameters.S = query;
                });

            if (IsSearchRequestOutdated(currentSequence))
                return;

            SearchResults = (response?.SearchModuleResponses ?? [])
                .Select(module => new SearchModuleGroup(
                    module.ModuleKey ?? string.Empty,
                    Math.Max(module.TotalCount ?? 0, module.Items?.Count ?? 0),
                    ResolveSearchHandlerResultTemplateType(module.ModuleKey ?? string.Empty),
                    (module.Items ?? [])
                    .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                    .Select(item => new SearchHandlerResultTemplateItem(
                        item.Title,
                        item.Description,
                        item.Identifier))
                    .ToArray()))
                .Where(module => module.Items.Count > 0)
                .ToArray();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception err)
        {
            Logger.LogError(err, "Error while executing frontend search for query '{Query}'", SearchText);
            SearchResults = [];
        }
        finally
        {
            if (!IsSearchRequestOutdated(currentSequence))
            {
                IsSearching = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private static string FormatModuleName(string moduleKey)
    {
        if (string.IsNullOrWhiteSpace(moduleKey))
            return "Search";

        string[] segments = moduleKey.Split('.',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0)
            return moduleKey;

        string moduleSegment = segments[^1];
        if (moduleSegment.EndsWith("SearchHandler", StringComparison.OrdinalIgnoreCase)
            && segments.Length > 1)
            moduleSegment = segments[^2];

        if (moduleSegment.EndsWith("Handler", StringComparison.OrdinalIgnoreCase))
            moduleSegment = moduleSegment[..^"Handler".Length];

        return moduleSegment.Humanize(LetterCasing.Title);
    }

    private Type? ResolveSearchHandlerResultTemplateType(string searchModuleKey)
    {
        SearchHandlerResultTemplateRegistration? registration = SearchHandlerResultTemplateRegistrations
            .FirstOrDefault(x => string.Equals(x.SearchModuleKey,
                searchModuleKey,
                StringComparison.Ordinal));

        if (registration is null)
            return null;

        return _availableSearchHandlerResultTemplateTypes.Contains(registration.SearchHandlerResultTemplateType)
            ? registration.SearchHandlerResultTemplateType
            : null;
    }

    private static IDictionary<string, object?> BuildSearchHandlerResultTemplateParameters(
        IReadOnlyList<SearchHandlerResultTemplateItem> items) =>
        new Dictionary<string, object?>
        {
            [nameof(ISearchHandlerResultTemplate.Items)] = items
        };

    public void Dispose()
    {
        _disposed = true;
        Interlocked.Increment(ref _searchSequence);
        SearchResults = [];
        IsSearching = false;
    }

    private void ResetSearchState()
    {
        Interlocked.Increment(ref _searchSequence);
        SearchResults = [];
        IsSearching = false;
        if (!_disposed)
            StateHasChanged();
    }

    private bool IsSearchRequestOutdated(long currentSequence) =>
        _disposed || currentSequence != _searchSequence;

    private sealed record SearchModuleGroup(
        string ModuleKey,
        int TotalCount,
        Type? TemplateType,
        IReadOnlyList<SearchHandlerResultTemplateItem> Items);
}
