namespace FaithTechTorontoAiBuildEvent.Domain.Participants;

public sealed class Participant
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string PublicLabel { get; set; }
    public string? Name { get; set; }
    public string? WhatYouMake { get; set; }
    public string? OnYourHeart { get; set; }
}
