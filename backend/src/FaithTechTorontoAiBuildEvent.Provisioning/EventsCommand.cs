using System.CommandLine;
namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class EventsCommand : Command
{
    public EventsCommand() : base("events", "Import event settings and schedules.") => Subcommands.Add(new ImportEventSeedCommand());
}
