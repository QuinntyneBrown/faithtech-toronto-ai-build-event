namespace FaithTechTorontoAiBuildEvent.Domain.Projects;

public sealed class Project
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? DemoUrl { get; set; }
}
