using FaithTechTorontoAiBuildEvent.Application.Validation;
using FaithTechTorontoAiBuildEvent.Domain.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class AuthenticateParticipantHandler(IParticipantStore store, IRequestSource source)
    : IRequestHandler<AuthenticateParticipantCommand, ParticipantSession?>
{
    public async Task<ParticipantSession?> Handle(AuthenticateParticipantCommand request, CancellationToken cancellationToken)
    {
        var normalized = EmailNormalizer.Normalize(request.Email);
        if (normalized is null) throw new InputValidationException("email", "Enter a valid email address.");
        if (string.IsNullOrWhiteSpace(request.EntryCode) || request.EntryCode.Length > 256) return null;
        return await store.Authenticate(request.EventId, normalized.Value.Email, normalized.Value.NormalizedEmail,
            request.EntryCode, source.Address, cancellationToken);
    }
}
