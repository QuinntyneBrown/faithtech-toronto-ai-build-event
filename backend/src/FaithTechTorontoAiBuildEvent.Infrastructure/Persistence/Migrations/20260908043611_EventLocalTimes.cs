using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventLocalTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndLocal",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EndOffsetMinutes",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndsAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartLocal",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartOffsetMinutes",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartsAtUtc",
                table: "Events",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Events",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndLocal",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EndOffsetMinutes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EndsAtUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StartLocal",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StartOffsetMinutes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StartsAtUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Events");
        }
    }
}
