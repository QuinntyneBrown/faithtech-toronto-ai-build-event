namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public interface IReadinessStore
{
    Task<bool> IsReady(CancellationToken cancellationToken);
}
