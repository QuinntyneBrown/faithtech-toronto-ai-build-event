using System.CommandLine;
using System.Security.Cryptography;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class ApplyOperationCommand : Command
{
    public ApplyOperationCommand() : base("apply", "Apply exactly one protected preview after explicit approval.")
    {
        var options = new OperatorOptions(); options.AddTo(this);
        var preview = new Argument<Guid>("preview-id"); var approve = new Option<Guid?>("--approve");
        Arguments.Add(preview); Options.Add(approve);
        SetAction((parse, token) => OperatorCommandRunner.Run(parse, options, token, async (session, cancellation) => {
            var id = parse.GetValue(preview); session.PreviewId = id;
            var saved = session.Files.Read<OperatorPreview>($"{id:N}.preview"); session.OperationId = saved.OperationId;
            using var operationLock = session.Files.Lock(saved.OperationId);
            OperatorApproval.Verify(session, saved);
            if (parse.GetValue(approve) != id) {
                if (parse.GetValue(approve) is not null || Console.IsInputRedirected) throw new OperationCanceledException();
                var target = $"{session.Principal.Server}/{session.Principal.Database}";
                Console.Error.WriteLine($"Type {target} to apply preview {id}:");
                if (Console.ReadLine() != target) throw new OperationCanceledException();
            }
            if (saved.FilePath is not null && Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(saved.FilePath, cancellation))) != saved.FileHash)
                throw new OperationConflictException();
            session.Files.Journal(saved.OperationId, new { saved.OperationId, saved.PreviewId, session.Principal, atUtc = DateTimeOffset.UtcNow, outcome = "started", saved.Kind });
            session.Started = true;
            if (saved.Kind == "migrate") {
                var history = await new SqlOperatorMigrations(session.Database).Apply(saved, migration =>
                    session.Files.Journal(saved.OperationId, new { saved.OperationId, migration, outcome = "migration-committed", atUtc = DateTimeOffset.UtcNow }), cancellation);
                session.Committed = true; session.Outcome = saved.PendingMigrations.Length == 0 ? "unchanged" : "succeeded";
                return new { appliedMigrations = history };
            }
            var result = await session.Sender.Send(new ApplyEventImportCommand(saved.OperationId, saved.Import ?? throw new ArgumentException("Not an import preview.")), cancellation);
            session.Committed = true;
            return result;
        }));
    }
}
