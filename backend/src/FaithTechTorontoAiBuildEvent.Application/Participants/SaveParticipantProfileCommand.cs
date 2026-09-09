using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record SaveParticipantProfileCommand(Guid OperationId, string ExpectedVersion, ProfileInput Input, string? Secret)
    : IRequest<ParticipantProfile>;
