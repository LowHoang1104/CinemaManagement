using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSeatIsActiveWithSeatStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Seats");

            migrationBuilder.AlterColumn<string>(
                name: "Format",
                table: "ShowTimes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeatStatusId",
                table: "Seats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SeatStatuses",
                columns: table => new
                {
                    SeatStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatStatuses", x => x.SeatStatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seats_SeatStatusId",
                table: "Seats",
                column: "SeatStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_SeatStatuses",
                table: "Seats",
                column: "SeatStatusId",
                principalTable: "SeatStatuses",
                principalColumn: "SeatStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_SeatStatuses",
                table: "Seats");

            migrationBuilder.DropTable(
                name: "SeatStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Seats_SeatStatusId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "SeatStatusId",
                table: "Seats");

            migrationBuilder.AlterColumn<string>(
                name: "Format",
                table: "ShowTimes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Seats",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
