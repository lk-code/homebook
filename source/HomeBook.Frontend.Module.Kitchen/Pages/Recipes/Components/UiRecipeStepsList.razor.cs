using HomeBook.Frontend.Module.Kitchen.ViewModels;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Module.Kitchen.Pages.Recipes.Components;

public partial class UiRecipeStepsList : ComponentBase
{
    [Parameter]
    public List<StepViewModel> Steps { get; set; } = [];

    [Parameter]
    public EventCallback<List<StepViewModel>> StepsChanged { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    private string _newStepDescription = string.Empty;
    private int _newStepHours = 0;
    private int _newStepMinutes = 0;

    protected override void OnParametersSet()
    {
        if (Steps is null)
            Steps = [];
    }

    private async Task AddStep()
    {
        if (string.IsNullOrWhiteSpace(_newStepDescription))
            return;

        int totalSeconds = (_newStepHours * 3600) + (_newStepMinutes * 60);

        Steps.Add(new StepViewModel
        {
            Description = _newStepDescription.Trim(),
            TimerDurationInSeconds = totalSeconds > 0 ? totalSeconds : null
        });

        _newStepDescription = string.Empty;
        _newStepHours = 0;
        _newStepMinutes = 0;

        await StepsChanged.InvokeAsync(Steps);
    }

    private async Task RemoveStep(StepViewModel step)
    {
        if (Steps.Remove(step))
            await StepsChanged.InvokeAsync(Steps);
    }

    private static int GetStepHours(StepViewModel step)
    {
        int seconds = step.TimerDurationInSeconds ?? 0;
        return seconds / 3600;
    }

    private static int GetStepMinutes(StepViewModel step)
    {
        int seconds = step.TimerDurationInSeconds ?? 0;
        return (seconds % 3600) / 60;
    }

    private static void SetStepHours(StepViewModel step,
        int hours)
    {
        int minutes = GetStepMinutes(step);
        int totalSeconds = Math.Max(0, hours) * 3600 + Math.Max(0, minutes) * 60;
        step.TimerDurationInSeconds = totalSeconds > 0 ? totalSeconds : null;
    }

    private static void SetStepMinutes(StepViewModel step,
        int minutes)
    {
        int hours = GetStepHours(step);
        int totalSeconds = Math.Max(0, hours) * 3600 + Math.Max(0, minutes) * 60;
        step.TimerDurationInSeconds = totalSeconds > 0 ? totalSeconds : null;
    }

    private static string FormatStepTimer(int? seconds)
    {
        if (seconds is null or <= 0)
            return "0m";

        TimeSpan span = TimeSpan.FromSeconds(seconds.Value);
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}h {span.Minutes}m";

        return $"{span.Minutes}m";
    }
}
