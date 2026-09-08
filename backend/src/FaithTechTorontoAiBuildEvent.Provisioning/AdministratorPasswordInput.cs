using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

internal sealed class AdministratorPasswordInput(Command command, bool allowDefault)
{
    public Option<bool> Prompt { get; } = new("--prompt-password") { Description = "Read a password from a masked terminal prompt." };
    public Option<bool> Stdin { get; } = new("--password-stdin") { Description = "Read one password line from redirected stdin." };

    public void Configure()
    {
        command.Options.Add(Prompt);
        command.Options.Add(Stdin);
        command.Validators.Add(result =>
        {
            var prompt = result.GetValue(Prompt);
            var stdin = result.GetValue(Stdin);
            if ((prompt && stdin) || (!allowDefault && !prompt && !stdin))
                result.AddError("Select exactly one of --prompt-password or --password-stdin.");
            if (prompt && Console.IsInputRedirected) result.AddError("A masked prompt requires an interactive terminal.");
            if (stdin && !Console.IsInputRedirected) result.AddError("--password-stdin requires redirected input.");
        });
    }

    public string Read(bool prompt, bool stdin)
    {
        var password = prompt || stdin ? SecretInput.ReadPassword() : "faithtech2026!";
        if (string.IsNullOrEmpty(password) || password.Length > 1024)
            throw new InvalidOperationException("Password must contain between 1 and 1024 characters.");
        return password;
    }
}
