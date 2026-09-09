namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public sealed record ProjectCard(
    Guid Id,
    string Title,
    string Description,
    string? RepositoryUrl,
    string? DemoUrl);
