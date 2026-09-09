using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record GetParticipantProfileQuery(string? Secret) : IRequest<ParticipantProfile?>;
