using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record AuthenticateAdministratorCommand(string Passcode, string Source) : IRequest<AdministratorAuthenticationResult>;
