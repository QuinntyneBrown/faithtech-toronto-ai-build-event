using FaithTechTorontoAiBuildEvent.Domain.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record GetAdministratorSessionQuery(Guid SessionId) : IRequest<AdministratorSession?>;
