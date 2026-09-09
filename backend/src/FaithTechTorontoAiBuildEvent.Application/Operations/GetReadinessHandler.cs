using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public sealed class GetReadinessHandler(IReadinessStore readinessStore) : IRequestHandler<GetReadinessQuery, bool>
{
    public Task<bool> Handle(GetReadinessQuery request, CancellationToken cancellationToken)
        => readinessStore.IsReadyAsync(cancellationToken);
}
