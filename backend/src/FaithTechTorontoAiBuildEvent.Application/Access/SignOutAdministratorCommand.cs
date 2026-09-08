using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record SignOutAdministratorCommand(Guid SessionId) : IRequest;
