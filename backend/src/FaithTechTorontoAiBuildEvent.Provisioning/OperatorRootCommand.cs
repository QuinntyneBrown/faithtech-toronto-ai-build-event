using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class OperatorRootCommand
{
    private readonly RootCommand root = new("Operate FaithTech event data through an explicitly selected database target.");

    public OperatorRootCommand()
    {
        root.Subcommands.Add(new MigrateCommand());
        root.Subcommands.Add(new TargetCommand());
        root.Subcommands.Add(new EventsCommand());
        root.Subcommands.Add(new OperationsCommand());
        root.Subcommands.Add(new CreateAdministratorCommand());
        root.Subcommands.Add(new AddUserCommand());
        root.Subcommands.Add(new ResetPasswordCommand());
        root.Subcommands.Add(new DisableAdministratorCommand());
        root.SetAction(_ => 0);
    }

    public async Task<int> InvokeAsync(string[] arguments)
    {
        var parsed = root.Parse(arguments);
        var result = await parsed.InvokeAsync();
        return parsed.Errors.Count > 0 ? 2 : result;
    }
}
