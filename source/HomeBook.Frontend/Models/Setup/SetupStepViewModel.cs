using HomeBook.Frontend.Abstractions.Contracts;
using MudBlazor;

namespace HomeBook.Frontend.Models.Setup;

public class SetupStepViewModel(string title, ISetupStep step) : IAsyncDisposable
{
    public string Title { get; set; } = title;
    public ISetupStep SetupStep { get; set; } = step;
    public bool HasError { get; set; } = false;
    public bool Completed { get; set; } = false;
    private MudStep? _stepRef;

    public event Func<MudStep?, Task>? OnStepRefSetAsync;
    public Func<Action, Task>? OnUIDispatchRequired { get; set; }

    public MudStep? StepRef
    {
        get => _stepRef;
        set
        {
            _stepRef = value;
            _ = OnStepRefSet();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_stepRef != null)
        {
            await _stepRef.DisposeAsync();
        }
    }

    private async Task OnStepRefSet()
    {
        if (OnStepRefSetAsync is not null)
            await OnStepRefSetAsync.Invoke(StepRef);

        if (StepRef is null)
            return;

        if (OnUIDispatchRequired != null)
        {
            await OnUIDispatchRequired.Invoke(async () =>
            {
                if (Completed)
                    await SetIsCompleted();

                if (HasError)
                    await SetHasError();
            });
        }
    }

    public async Task SetIsCompleted()
    {
        StepRef.Completed = true;
    }

    public async Task SetHasError()
    {
        await StepRef.SetHasErrorAsync(true, true);
    }
}
