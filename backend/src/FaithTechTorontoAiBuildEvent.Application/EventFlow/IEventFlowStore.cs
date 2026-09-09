namespace FaithTechTorontoAiBuildEvent.Application.EventFlow;

public interface IEventFlowStore
{
    Task<bool> AdvanceAsync(Guid operationId, byte[] inputDigest, long expectedVersion, string fromScreen, string toScreen, CancellationToken cancellationToken);
}
