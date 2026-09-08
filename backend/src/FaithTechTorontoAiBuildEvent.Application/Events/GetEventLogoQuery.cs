using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record GetEventLogoQuery(Guid EventId) : IRequest<LogoContent?>;
