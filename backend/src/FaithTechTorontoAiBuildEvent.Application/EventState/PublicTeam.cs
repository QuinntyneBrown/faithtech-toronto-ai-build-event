namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public sealed record PublicTeam(Guid Id, string Label, Guid? ProjectId, IReadOnlyList<PublicTeamMember> Members);
