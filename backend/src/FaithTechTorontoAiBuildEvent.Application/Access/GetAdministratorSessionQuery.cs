using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record GetAdministratorSessionQuery(string? Secret) : IRequest<bool>;
