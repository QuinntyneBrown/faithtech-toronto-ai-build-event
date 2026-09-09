using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record GetParticipantSessionQuery(string? Secret) : IRequest<ParticipantSessionState?>;
