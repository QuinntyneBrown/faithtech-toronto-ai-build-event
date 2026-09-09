using System.Data;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class OperatorRecovery
{
    public static async Task<OperatorResult?> Resolve(OperatorSession session, double duration, bool cancelled)
    {
        if (session.CurrentPreview is not { } preview) return null;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try {
            var db = session.Database; db.ChangeTracker.Clear(); db.Database.SetCommandTimeout(5);
            var connection = (SqlConnection)db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) { await connection.CloseAsync(); await connection.OpenAsync(timeout.Token); }
            var identity = await SqlOperatorConnection.Identify(connection, session.Principal.Environment, 5, timeout.Token);
            if (identity.Key != preview.Principal.Key) return null;
            object? data; string outcome;
            if (preview.Kind == "import") {
                data = await session.Sender.Send(new ReconcileEventImportQuery(preview.OperationId), timeout.Token);
                outcome = data is not null ? "reconciled" : cancelled ? "cancelled" : "rejected";
            } else {
                var history = (await db.Database.GetAppliedMigrationsAsync(timeout.Token)).ToArray();
                var expected = preview.AppliedMigrations.Concat(preview.PendingMigrations).ToArray();
                if (!history.SequenceEqual(expected.Take(history.Length)) || history.Length < preview.AppliedMigrations.Length) return null;
                var complete = history.Length == expected.Length;
                outcome = complete ? "reconciled" : history.Length > preview.AppliedMigrations.Length ? "partial" : cancelled ? "cancelled" : "rejected";
                data = new { appliedMigrations = history, remainingMigrations = expected.Except(history).ToArray() };
            }
            session.Committed = outcome == "reconciled";
            session.ExitCode = outcome == "reconciled" ? 0 : outcome == "cancelled" ? 5 : 1;
            return new(preview.OperationId, preview.PreviewId, session.Principal, outcome, duration, data,
                outcome == "reconciled" ? null : "Reconciliation confirmed the reported outcome; no mutation was replayed.");
        } catch (Exception error) when (error is SqlException or DbUpdateException or InvalidOperationException or OperationCanceledException or UnauthorizedAccessException) { return null; }
    }
}
