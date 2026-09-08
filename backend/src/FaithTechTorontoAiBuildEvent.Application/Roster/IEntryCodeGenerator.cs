namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public interface IEntryCodeGenerator
{
    string Generate();
    string Digest(Guid eventId, string code);
}
