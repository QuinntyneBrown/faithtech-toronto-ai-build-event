using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class OperatorRootCommand
{
    private readonly RootCommand root = new("Operate FaithTech event data through an explicitly selected database target.");

    public OperatorRootCommand()
    {
        root.Subcommands.Add(new MigrateCommand());
        root.Subcommands.Add(new CreateAdministratorCommand());
        root.Subcommands.Add(new AddUserCommand());
        root.Subcommands.Add(new ResetPasswordCommand());
        root.Subcommands.Add(new DisableAdministratorCommand());
        root.SetAction(_ => 0);
    }

    public Task<int> InvokeAsync(string[] arguments) => root.Parse(arguments).InvokeAsync();
}
