using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OperatorAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OperationReceipts_ActorId_EventId_OperationId",
                table: "OperationReceipts");

            migrationBuilder.AddColumn<int>(
                name: "ActorKind",
                table: "OperationReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ActorKind",
                table: "AuditRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OperatorIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrincipalKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorIdentities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationReceipts_ActorKind_ActorId_EventId_OperationId",
                table: "OperationReceipts",
                columns: new[] { "ActorKind", "ActorId", "EventId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperatorIdentities_PrincipalKey",
                table: "OperatorIdentities",
                column: "PrincipalKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatorIdentities");

            migrationBuilder.DropIndex(
                name: "IX_OperationReceipts_ActorKind_ActorId_EventId_OperationId",
                table: "OperationReceipts");

            migrationBuilder.DropColumn(
                name: "ActorKind",
                table: "OperationReceipts");

            migrationBuilder.DropColumn(
                name: "ActorKind",
                table: "AuditRecords");

            migrationBuilder.CreateIndex(
                name: "IX_OperationReceipts_ActorId_EventId_OperationId",
                table: "OperationReceipts",
                columns: new[] { "ActorId", "EventId", "OperationId" },
                unique: true);
        }
    }
}
