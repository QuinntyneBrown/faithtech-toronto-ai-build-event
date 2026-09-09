namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record AdministratorParticipant(Guid Id, string Email, string PublicLabel, string? Name, string? WhatYouMake, string? OnYourHeart, string? TeamLabel, bool HasWonRaffle);
