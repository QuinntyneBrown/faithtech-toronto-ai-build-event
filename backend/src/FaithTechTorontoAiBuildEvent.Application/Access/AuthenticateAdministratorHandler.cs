using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class AuthenticateAdministratorHandler(
    IAdministratorSessionStore sessionStore,
    IAdministratorLoginAttemptStore loginAttempts,
    IEntryReceiptSecretService secretService,
    ISourceDigestService sourceDigestService)
    : IRequestHandler<AuthenticateAdministratorCommand, AdministratorAuthenticationResult>
{
    public async Task<AdministratorAuthenticationResult> Handle(AuthenticateAdministratorCommand request, CancellationToken cancellationToken)
    {
        var source = sourceDigestService.Digest(request.Source);
        var retryAfter = await loginAttempts.GetRetryAfterAsync(source, DateTimeOffset.UtcNow, cancellationToken);
        if (retryAfter is not null) throw new AdministratorAuthenticationThrottledException(retryAfter.Value);
        if (request.Passcode.Length != 4 || request.Passcode.Any(character => character is < '0' or > '9'))
        {
            await loginAttempts.RecordFailureAsync(source, DateTimeOffset.UtcNow, cancellationToken);
            return new AdministratorAuthenticationResult(false, null);
        }

        var sessionSecret = secretService.CreateSecret();
        var result = await sessionStore.AuthenticateAsync(request.Passcode, sessionSecret, DateTimeOffset.UtcNow, cancellationToken);
        if (!result.Authenticated) await loginAttempts.RecordFailureAsync(source, DateTimeOffset.UtcNow, cancellationToken);
        return result;
    }
}
