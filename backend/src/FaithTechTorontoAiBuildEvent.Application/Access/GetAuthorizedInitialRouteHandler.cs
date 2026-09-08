using FaithTechTorontoAiBuildEvent.Application.Events;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class GetAuthorizedInitialRouteHandler(IEventStore store) : IRequestHandler<GetAuthorizedInitialRouteQuery, string>
{
    public Task<string> Handle(GetAuthorizedInitialRouteQuery request, CancellationToken cancellationToken) =>
        store.GetAuthorizedInitialRoute(request.EventId, request.RequestedReturnTo, cancellationToken);
}
