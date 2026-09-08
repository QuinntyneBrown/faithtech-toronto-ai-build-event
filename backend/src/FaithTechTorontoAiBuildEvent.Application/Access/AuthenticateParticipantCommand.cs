using FaithTechTorontoAiBuildEvent.Domain.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record AuthenticateParticipantCommand(Guid EventId, string? Email, string? EntryCode) : IRequest<ParticipantSession?>;
