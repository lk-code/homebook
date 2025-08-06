using HomeBook.Frontend.Abstractions.Contracts;
using MudBlazor;

namespace HomeBook.Frontend.Models.Setup;

public class SetupStepViewModel(string title, ISetupStep step)
{
    public string Title { get; set; } = title;
    public ISetupStep SetupStep { get; set; } = step;
    public bool HasError { get; set; } = false;
    public bool Completed { get; set; } = false;
    public MudStep? StepRef { get; set; }
}
