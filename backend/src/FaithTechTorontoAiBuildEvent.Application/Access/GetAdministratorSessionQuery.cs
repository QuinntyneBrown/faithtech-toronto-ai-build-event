using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record GetAdministratorSessionQuery(Guid SessionId) : IRequest<AdministratorSessionState?>;
