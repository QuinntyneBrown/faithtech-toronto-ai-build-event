using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateSessionInvalidationProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.InvalidatePrivateSessions
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;
                    BEGIN TRANSACTION;
                    UPDATE dbo.AdministratorSessions SET Revoked = 1 WHERE Revoked = 0;
                    UPDATE dbo.ParticipantSessions SET Revoked = 1 WHERE Revoked = 0;
                    UPDATE dbo.EntryReceipts SET Revoked = 1 WHERE Revoked = 0;
                    COMMIT TRANSACTION;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.InvalidatePrivateSessions;");
        }
    }
}
