using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PresentationWindow_EndLocal",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresentationWindow_EndOffsetMinutes",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PresentationWindow_EndsAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PresentationWindow_StartLocal",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresentationWindow_StartOffsetMinutes",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PresentationWindow_StartsAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SelectionClosedAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SelectionOpenedAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SelectionWindow_EndLocal",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SelectionWindow_EndOffsetMinutes",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SelectionWindow_EndsAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SelectionWindow_StartLocal",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SelectionWindow_StartOffsetMinutes",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SelectionWindow_StartsAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Stages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ScreenType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    ResourceUrl = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    Interval_StartLocal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Interval_EndLocal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Interval_StartOffsetMinutes = table.Column<int>(type: "int", nullable: false),
                    Interval_EndOffsetMinutes = table.Column<int>(type: "int", nullable: false),
                    Interval_StartsAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Interval_EndsAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stages_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stages_EventId",
                table: "Stages",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stages");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PresentationWindow_EndLocal",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PresentationWindow_EndOffsetMinutes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PresentationWindow_EndsAtUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PresentationWindow_StartLocal",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PresentationWindow_StartOffsetMinutes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PresentationWindow_StartsAtUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SelectionClosedAtUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SelectionOpenedAtUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SelectionWindow_EndLocal",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SelectionWindow_EndOffsetMinutes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SelectionWindow_EndsAtUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SelectionWindow_StartLocal",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SelectionWindow_StartOffsetMinutes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SelectionWindow_StartsAtUtc",
                table: "Events");
        }
    }
}
