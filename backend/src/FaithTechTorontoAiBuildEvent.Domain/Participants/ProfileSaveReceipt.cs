namespace FaithTechTorontoAiBuildEvent.Domain.Participants;

public sealed class ProfileSaveReceipt
{
    public Guid OperationId { get; set; }
    public Guid ParticipantId { get; set; }
    public required byte[] InputDigest { get; set; }
    public string? Name { get; set; }
    public string? WhatYouMake { get; set; }
    public string? OnYourHeart { get; set; }
}
