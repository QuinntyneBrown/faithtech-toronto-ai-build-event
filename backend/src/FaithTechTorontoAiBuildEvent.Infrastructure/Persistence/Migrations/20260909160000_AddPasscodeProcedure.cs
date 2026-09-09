using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CompanionDbContext))]
[Migration("20260909160000_AddPasscodeProcedure")]
public sealed class AddPasscodeProcedure : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE OR ALTER PROCEDURE dbo.ReplaceAdminPasscode
                @Passcode nvarchar(max)
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                IF @Passcode IS NULL OR DATALENGTH(@Passcode) <> 8
                    THROW 50000, 'Passcode must contain exactly four ASCII digits.', 1;
                IF @Passcode COLLATE Latin1_General_100_BIN2 LIKE '%[^0-9]%'
                    THROW 50000, 'Passcode must contain exactly four ASCII digits.', 1;

                BEGIN TRANSACTION;
                DECLARE @lockResult int;
                EXEC @lockResult = sys.sp_getapplock @Resource = 'FaithTech.Companion.Passcode', @LockMode = 'Exclusive', @LockOwner = 'Transaction';
                IF @lockResult < 0
                    THROW 50001, 'Could not acquire credential update lock.', 1;

                DECLARE @salt varbinary(32) = CRYPT_GEN_RANDOM(32);
                DECLARE @digits varchar(4) = CONVERT(varchar(4), @Passcode);
                DECLARE @verifier varbinary(64) = HASHBYTES('SHA2_512', CONVERT(varbinary(max), 'FaithTech.Companion.Passcode.v1:') + @salt + CONVERT(varbinary(4), @digits));
                DECLARE @changedAtUtc datetimeoffset = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00');
                DECLARE @revision bigint;

                IF EXISTS (SELECT 1 FROM dbo.CompanionCredentials WHERE Id = 1)
                BEGIN
                    UPDATE dbo.CompanionCredentials
                    SET Salt = @salt, Verifier = @verifier, Revision = Revision + 1, ChangedAtUtc = @changedAtUtc
                    WHERE Id = 1;
                END
                ELSE
                BEGIN
                    INSERT dbo.CompanionCredentials (Salt, Verifier, Revision, ChangedAtUtc)
                    VALUES (@salt, @verifier, 1, @changedAtUtc);
                END

                UPDATE dbo.AdministratorSessions SET Revoked = 1 WHERE Revoked = 0;
                SELECT @revision = Revision FROM dbo.CompanionCredentials WHERE Id = 1;
                COMMIT TRANSACTION;
                SELECT @revision AS Revision, @changedAtUtc AS ChangedAtUtc;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.ReplaceAdminPasscode;");
}
