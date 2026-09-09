using System.CommandLine;
using System.Security.Cryptography;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class OperatorSession : IAsyncDisposable
{
    private readonly SqlConnection connection;
    private readonly IHost host;
    private readonly IServiceScope scope;
    public OperatorPrincipal Principal { get; }
    public ProtectedOperatorFiles Files { get; }
    public EventDbContext Database => scope.ServiceProvider.GetRequiredService<EventDbContext>();
    public ISender Sender => scope.ServiceProvider.GetRequiredService<ISender>();
    public Guid? OperationId { get; set; }
    public Guid? PreviewId { get; set; }
    public bool Started { get; set; }
    public bool Committed { get; set; }
    public string Outcome { get; set; } = "succeeded";
    public int ExitCode { get; set; }
    public static string AssemblyHash => Convert.ToHexString(SHA256.HashData([
        .. File.ReadAllBytes(typeof(EventDbContext).Assembly.Location),
        .. File.ReadAllBytes(typeof(EventSeedReader).Assembly.Location),
        .. File.ReadAllBytes(typeof(Program).Assembly.Location)]));

    private OperatorSession(SqlConnection connection, OperatorPrincipal principal, string config, int timeout)
    {
        this.connection = connection; Principal = principal;
        Files = new(Path.Combine(Path.GetDirectoryName(config)!, Path.GetFileNameWithoutExtension(config) + ".state"));
        var builder = Host.CreateApplicationBuilder(); builder.Logging.ClearProviders();
        builder.Services.AddSingleton(principal);
        builder.Services.AddDbContext<EventDbContext>(options => options.UseSqlServer(connection, sql => sql.CommandTimeout(timeout)));
        builder.Services.AddMediatR(options => {
            options.TypeEvaluator = type => type == typeof(ReviewEventImportHandler) || type == typeof(ApplyEventImportHandler) || type == typeof(ReconcileEventImportHandler);
            options.RegisterServicesFromAssemblyContaining<ReviewEventImportQuery>();
        });
        builder.Services.AddScoped<IEventImportStore, SqlEventImportStore>();
        host = builder.Build(); scope = host.Services.CreateScope();
    }
    public static async Task<OperatorSession> Open(ParseResult result, OperatorOptions options, CancellationToken token)
    {
        var config = result.GetValue(options.Config)!;
        if (!Path.IsPathFullyQualified(config)) throw new ArgumentException("Use an absolute config path.");
        var profile = new OperatorTargetProfileStore(config).Load(result.GetValue(options.Target)!);
        var timeout = result.GetValue(options.CommandTimeout);
        if (timeout <= 0) throw new ArgumentException("Command timeout must be positive.");
        var secret = Environment.GetEnvironmentVariable(profile.ConnectionEnvironmentVariable ?? "") ?? throw new ArgumentException("Supply the named connection environment variable.");
        var connection = new SqlConnection(SqlOperatorConnection.Configure(secret, profile.Server, profile.Database, profile.Environment, result.GetValue(options.ConnectTimeout)));
        try {
            await connection.OpenAsync(token);
            var principal = await SqlOperatorConnection.Identify(connection, profile.Environment, timeout, token);
            if (!principal.Database.Equals(profile.Database, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Resolved database mismatch.");
            return new(connection, principal, config, timeout);
        } catch { await connection.DisposeAsync(); throw; }
    }
    public async ValueTask DisposeAsync() { scope.Dispose(); host.Dispose(); await connection.DisposeAsync(); }
}
