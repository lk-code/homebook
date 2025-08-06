using MudBlazor;

namespace HomeBook.Frontend.Models.Setup;

public class SetupStepViewModel(string title)
{
    public string Title { get; set; } = title;
    public bool HasError { get; set; } = false;
    public bool Completed { get; set; } = false;
    public MudStep? StepRef { get; set; }
}
