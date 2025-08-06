using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Services.Services;

public class SetupService : ISetupService
{
    private bool _isDone = false;

    public Guid ServiceId { get; } = Guid.NewGuid();

    public void SetIsDone(bool value) => _isDone = value;

    public async Task<bool> IsSetupDoneAsync() => _isDone;
}
