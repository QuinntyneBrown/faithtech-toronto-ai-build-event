using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public sealed class GetPublicEventSnapshotHandler(IEventStateStore eventStateStore)
    : IRequestHandler<GetPublicEventSnapshotQuery, PublicEventSnapshot>
{
    public Task<PublicEventSnapshot> Handle(GetPublicEventSnapshotQuery request, CancellationToken cancellationToken)
        => eventStateStore.GetPublicSnapshotAsync(cancellationToken);
}
