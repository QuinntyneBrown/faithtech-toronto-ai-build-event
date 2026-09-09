namespace FaithTechTorontoAiBuildEvent.Domain.Teams;

public sealed class Team
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public Guid? ProjectId { get; set; }
}
