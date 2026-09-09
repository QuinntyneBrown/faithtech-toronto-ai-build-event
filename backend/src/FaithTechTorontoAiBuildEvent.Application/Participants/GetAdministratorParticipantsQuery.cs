using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record GetAdministratorParticipantsQuery(string? AdministratorSecret) : IRequest<IReadOnlyList<AdministratorParticipant>>;
