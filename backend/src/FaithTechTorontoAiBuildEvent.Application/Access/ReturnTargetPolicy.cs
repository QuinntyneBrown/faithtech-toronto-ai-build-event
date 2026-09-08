namespace FaithTechTorontoAiBuildEvent.Application.Access;

/// <summary>Accepts only a same-event relative route recognized by the client's core navigation; an external URL,
/// another event's route, or anything unrecognized falls back to the authorized default rather than being honored.</summary>
public static class ReturnTargetPolicy
{
    private static readonly string[] KnownSuffixes = ["", "schedule", "teams", "people", "messages", "quiz", "raffle", "showcase"];

    public static string Resolve(Guid eventId, string? requested, string authorizedDefault)
    {
        if (string.IsNullOrEmpty(requested) || requested[0] != '/' || requested.StartsWith("//", StringComparison.Ordinal))
            return authorizedDefault;
        var prefix = $"/events/{eventId:D}";
        if (!requested.StartsWith(prefix, StringComparison.Ordinal)) return authorizedDefault;
        var remainder = requested[prefix.Length..].Trim('/');
        return Array.IndexOf(KnownSuffixes, remainder) >= 0 ? requested : authorizedDefault;
    }
}
