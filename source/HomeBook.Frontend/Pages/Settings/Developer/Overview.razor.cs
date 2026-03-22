using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Pages.Settings.Developer;

public partial class Overview : ComponentBase
{
    private readonly List<KeyValuePair<string, string?>> _configurationBackendValues = new();
    private readonly List<KeyValuePair<string, string?>> _configurationFrontendValues = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        CancellationToken cancellationToken = CancellationToken.None;
        await LoadConfigurationValuesAsync(cancellationToken);
    }

    private async Task LoadConfigurationValuesAsync(CancellationToken cancellationToken)
    {
        // load backend values
        await LoadBackendConfigurationValuesAsync(cancellationToken);

        // load frontend values
        await LoadFrontendConfigurationValuesAsync(cancellationToken);
    }

    private async Task LoadFrontendConfigurationValuesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _configurationFrontendValues.Clear();
            List<KeyValuePair<string, string?>> values = Configuration
                .AsEnumerable()
                .Where(x => x.Value != null)
                .ToList();

            _configurationFrontendValues.AddRange(values);

            StateHasChanged();
        }
        catch (Exception err)
        {
            // show error
        }
        finally
        {
        }
    }

    private async Task LoadBackendConfigurationValuesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _configurationBackendValues.Clear();
            List<KeyValuePair<string, string?>> values = await DeveloperService
                .GetBackendConfigurationAsync(cancellationToken);

            _configurationBackendValues.AddRange(values);

            StateHasChanged();
        }
        catch (Exception err)
        {
            // show error
        }
        finally
        {
        }
    }
}
