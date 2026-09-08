using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventRoster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "AuditRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(640)", maxLength: 640, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(640)", maxLength: 640, nullable: true),
                    CodeDigest = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CredentialVersion = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeIssuedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FirstAccessAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_EventId_CodeDigest",
                table: "Registrations",
                columns: new[] { "EventId", "CodeDigest" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_EventId_NormalizedEmail",
                table: "Registrations",
                columns: new[] { "EventId", "NormalizedEmail" },
                unique: true,
                filter: "[Active] = 1 AND [NormalizedEmail] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "AuditRecords");
        }
    }
}
