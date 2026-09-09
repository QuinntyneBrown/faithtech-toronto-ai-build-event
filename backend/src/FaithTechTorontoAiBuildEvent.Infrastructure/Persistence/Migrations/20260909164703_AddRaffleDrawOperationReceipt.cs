using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRaffleDrawOperationReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ExpectedVersion",
                table: "RaffleDraws",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "RaffleDraws",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE dbo.RaffleDraws SET OperationId = NEWID() WHERE OperationId = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.CreateIndex(
                name: "IX_RaffleDraws_OperationId",
                table: "RaffleDraws",
                column: "OperationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaffleDraws_OperationId",
                table: "RaffleDraws");

            migrationBuilder.DropColumn(
                name: "ExpectedVersion",
                table: "RaffleDraws");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "RaffleDraws");
        }
    }
}
