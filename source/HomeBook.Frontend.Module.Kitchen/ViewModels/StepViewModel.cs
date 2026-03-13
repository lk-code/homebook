namespace HomeBook.Frontend.Module.Kitchen.ViewModels;

public class StepViewModel
{
    public Guid Id { get; set; }
    public string Description { get; set; } =  string.Empty;
    public int? TimerDurationInSeconds { get; set; }
}
