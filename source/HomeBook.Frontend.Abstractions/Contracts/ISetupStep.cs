namespace HomeBook.Frontend.Abstractions.Contracts;

public interface ISetupStep
{
    public string Key { get; }
    Task HandleStepAsync();
}
