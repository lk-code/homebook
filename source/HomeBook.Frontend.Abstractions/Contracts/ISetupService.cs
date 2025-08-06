namespace HomeBook.Frontend.Abstractions.Contracts;

public interface ISetupService
{
    Guid ServiceId { get; }
    void SetIsDone(bool value);
    Task<bool> IsSetupDoneAsync();
}
