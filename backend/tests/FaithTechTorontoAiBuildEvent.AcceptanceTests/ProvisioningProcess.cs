using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

internal static class ProvisioningProcess
{
    public static async Task<int> Run(EventApiFactory factory, string[] arguments, string? password = null)
        => (await Execute(factory, arguments, password)).ExitCode;

    public static async Task<ProvisioningResult> Execute(EventApiFactory factory, string[] arguments, string? password = null,
        bool closeOutputBeforeMutation = false, bool closeOutputImmediately = false)
    {
        using var scope = factory.Services.CreateScope();
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        };
        if (Environment.GetEnvironmentVariable("FAITHTECH_TEST_TOOL") is { Length: > 0 } installedTool)
            start.FileName = installedTool;
        else
        {
            start.ArgumentList.Add("exec");
            start.ArgumentList.Add("--runtimeconfig");
            start.ArgumentList.Add(Path.ChangeExtension(typeof(ProvisioningProcess).Assembly.Location, "runtimeconfig.json"));
            start.ArgumentList.Add("--depsfile");
            start.ArgumentList.Add(Path.ChangeExtension(typeof(ProvisioningProcess).Assembly.Location, "deps.json"));
            start.ArgumentList.Add(typeof(Provisioning.Program).Assembly.Location);
        }
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["ConnectionStrings__EventDatabase"] = scope.ServiceProvider
            .GetRequiredService<EventDbContext>().Database.GetConnectionString();
        start.Environment["Security__DigestKey"] = Convert.ToBase64String(new byte[32]);
        using var process = Process.Start(start)!;
        if (closeOutputImmediately) process.StandardOutput.Close();
        if (closeOutputBeforeMutation)
        {
            await process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(30));
            process.StandardOutput.Close();
        }
        var output = closeOutputBeforeMutation || closeOutputImmediately ? Task.FromResult("") : process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (password is not null) await process.StandardInput.WriteLineAsync(password);
        process.StandardInput.Close();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try { await process.WaitForExitAsync(timeout.Token); }
        finally { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        await Task.WhenAll(output, error);
        return new(process.ExitCode, await output, await error);
    }
}
