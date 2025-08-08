namespace HomeBook.Frontend.Abstractions.Contracts;

public interface ISetupStep
{
    public string Key { get; }
    public bool HasError { get; set; }
    public bool IsSuccessful { get; set; }
    Task HandleStepAsync();
    Task<bool> IsStepDoneAsync(CancellationToken cancellationToken);
}
