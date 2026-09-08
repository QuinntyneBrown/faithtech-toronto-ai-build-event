using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventLogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LogoId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Logos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Bytes = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    MediaType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_LogoId",
                table: "Events",
                column: "LogoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Logos_LogoId",
                table: "Events",
                column: "LogoId",
                principalTable: "Logos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Logos_LogoId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "Logos");

            migrationBuilder.DropIndex(
                name: "IX_Events_LogoId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LogoId",
                table: "Events");
        }
    }
}
