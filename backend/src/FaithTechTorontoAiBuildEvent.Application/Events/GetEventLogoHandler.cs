using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class GetEventLogoHandler(IEventLogoStore store) : IRequestHandler<GetEventLogoQuery, LogoContent?>
{
    public Task<LogoContent?> Handle(GetEventLogoQuery request, CancellationToken cancellationToken) => store.Get(request.EventId, cancellationToken);
}
