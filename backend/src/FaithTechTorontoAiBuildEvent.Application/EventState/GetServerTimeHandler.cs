using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public sealed class GetServerTimeHandler(IEventStateStore eventStateStore)
    : IRequestHandler<GetServerTimeQuery, DateTimeOffset>
{
    public Task<DateTimeOffset> Handle(GetServerTimeQuery request, CancellationToken cancellationToken)
        => eventStateStore.GetServerTimeAsync(cancellationToken);
}
