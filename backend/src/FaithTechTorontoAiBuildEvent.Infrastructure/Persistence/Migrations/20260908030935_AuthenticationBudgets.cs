using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuthenticationBudgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthenticationFailures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AccountKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FailedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationFailures", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationFailures_AccountKey_FailedAtUtc",
                table: "AuthenticationFailures",
                columns: new[] { "AccountKey", "FailedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationFailures_SourceKey_FailedAtUtc",
                table: "AuthenticationFailures",
                columns: new[] { "SourceKey", "FailedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationFailures");
        }
    }
}
