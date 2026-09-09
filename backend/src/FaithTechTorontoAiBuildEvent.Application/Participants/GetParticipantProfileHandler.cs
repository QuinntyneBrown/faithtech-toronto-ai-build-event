using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class GetParticipantProfileHandler(IProfileStore profileStore, IEntryReceiptSecretService secretService)
    : IRequestHandler<GetParticipantProfileQuery, ParticipantProfile?>
{
    public Task<ParticipantProfile?> Handle(GetParticipantProfileQuery request, CancellationToken cancellationToken)
        => string.IsNullOrEmpty(request.Secret)
            ? Task.FromResult<ParticipantProfile?>(null)
            : profileStore.GetAsync(secretService.Digest(request.Secret), DateTimeOffset.UtcNow, cancellationToken);
}
