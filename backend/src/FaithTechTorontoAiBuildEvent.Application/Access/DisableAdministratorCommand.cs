using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record DisableAdministratorCommand(string Username) : IRequest;
