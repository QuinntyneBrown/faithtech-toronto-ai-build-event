using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record GetAuthorizedInitialRouteQuery(Guid EventId, string? RequestedReturnTo) : IRequest<string>;
