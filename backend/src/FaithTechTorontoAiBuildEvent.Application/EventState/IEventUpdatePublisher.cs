namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public interface IEventUpdatePublisher
{
    Task PublishAsync(long version, CancellationToken cancellationToken);
}
