using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.UI.Components;

public partial class UiNumericGroup<T> : ComponentBase
{
    private MudNumericField<T> _numericInput = null!;

    protected T _value = default!;

    [Parameter]
    public T Value
    {
        get => _value;
        set => _value = value;
    }

    [Parameter]
    public T Min { get; set; } = default!;

    [Parameter]
    public T Max { get; set; } = default!;

    [Parameter]
    public T Step { get; set; } = default!;

    [Parameter]
    public EventCallback<T> ValueChanged { get; set; }

    private async Task OnValueChanged(T? value)
    {
        await ValueChanged.InvokeAsync(value);
        StateHasChanged();
    }

    public async Task Decrement()
    {
        await _numericInput.Decrement();
        StateHasChanged();
    }

    public async Task Increment()
    {
        await _numericInput.Increment();
        StateHasChanged();
    }
}
