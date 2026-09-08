using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record GetParticipantSessionQuery(Guid SessionId) : IRequest<ParticipantSessionState?>;
