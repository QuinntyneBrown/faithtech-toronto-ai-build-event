using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record RevokeAdministratorSessionCommand(string? SessionSecret) : IRequest;
