using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class AuthenticateAdministratorHandler(
    IAdministratorSessionStore sessionStore,
    IEntryReceiptSecretService secretService)
    : IRequestHandler<AuthenticateAdministratorCommand, AdministratorAuthenticationResult>
{
    public async Task<AdministratorAuthenticationResult> Handle(AuthenticateAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (request.Passcode.Length != 4 || request.Passcode.Any(character => character is < '0' or > '9'))
        {
            return new AdministratorAuthenticationResult(false, null);
        }

        var sessionSecret = secretService.CreateSecret();
        return await sessionStore.AuthenticateAsync(request.Passcode, sessionSecret, DateTimeOffset.UtcNow, cancellationToken);
    }
}
