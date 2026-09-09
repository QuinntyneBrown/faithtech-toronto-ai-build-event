namespace FaithTechTorontoAiBuildEvent.Domain.Participants;

public static class ParticipantPublicDisplay
{
    public static string Format(string? name, string publicLabel)
        => string.IsNullOrWhiteSpace(name) ? publicLabel : $"{name} ({publicLabel})";
}
