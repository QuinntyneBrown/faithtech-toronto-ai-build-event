namespace FaithTechTorontoAiBuildEvent.Application.EventFlow;

public interface IEventFlowStore
{
    Task<bool> AdvanceAsync(long expectedVersion, string fromScreen, string toScreen, CancellationToken cancellationToken);
}
