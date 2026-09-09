namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public interface IReadinessStore
{
    Task<bool> IsReadyAsync(CancellationToken cancellationToken);
}
