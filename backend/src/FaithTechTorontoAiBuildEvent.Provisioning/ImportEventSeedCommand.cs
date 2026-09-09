using System.CommandLine;
using System.Security.Cryptography;
using FaithTechTorontoAiBuildEvent.Application.Operations;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class ImportEventSeedCommand : Command
{
    public ImportEventSeedCommand() : base("import", "Preview a validated seed import; apply the returned preview explicitly.")
    {
        var options = new OperatorOptions(); options.AddTo(this);
        var create = new Option<bool>("--create"); var eventId = new Option<Guid?>("--event");
        var version = new Option<string>("--version"); var file = new Option<string>("--file") { Required = true };
        var preview = new Option<bool>("--preview"); var operation = new Option<Guid?>("--operation-id");
        Options.Add(create); Options.Add(eventId); Options.Add(version); Options.Add(file); Options.Add(preview); Options.Add(operation);
        Validators.Add(result => {
            if (!result.GetValue(preview) || result.GetValue(create) == result.GetValue(eventId).HasValue ||
                (result.GetValue(create) ? result.GetValue(version) is not null : string.IsNullOrWhiteSpace(result.GetValue(version))) ||
                result.GetValue(operation) == Guid.Empty || result.GetValue(eventId) == Guid.Empty)
                result.AddError("Use --preview and exactly one of --create or --event with --version; identities must be nonempty.");
        });
        SetAction((parse, token) => OperatorCommandRunner.Run(parse, options, token, async (session, cancellation) => {
            var op = parse.GetValue(operation) ?? Guid.NewGuid(); session.OperationId = op;
            using var operationLock = session.Files.Lock(op);
            var id = parse.GetValue(eventId) ?? (session.Files.Exists($"{op:N}.operation")
                ? session.Files.Read<OperatorPreview>($"{op:N}.operation").Import?.EventId ?? Guid.NewGuid() : Guid.NewGuid());
            var path = Path.GetFullPath(parse.GetValue(file)!); var bytes = await File.ReadAllBytesAsync(path, cancellation);
            var review = await session.Sender.Send(new ReviewEventImportQuery(id, parse.GetValue(create), parse.GetValue(version), bytes), cancellation);
            var result = new OperatorPreview(1, Guid.NewGuid(), op, "import", session.Principal, DateTimeOffset.UtcNow,
                OperatorSession.AssemblyHash, path, Convert.ToHexString(SHA256.HashData(bytes)), review, [], []);
            session.PreviewId = result.PreviewId; session.Outcome = "previewed";
            session.Files.Write($"{result.PreviewId:N}.preview", result); session.Files.Write($"{op:N}.operation", result);
            return result;
        }));
    }
}
