using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class SimplifySeatStatusTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SeatStatuses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SeatStatuses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SeatStatuses");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SeatStatuses");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "SeatStatuses");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "SeatStatuses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SeatStatuses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatStatuses",
                type: "timestamp(6) with time zone",
                precision: 6,
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SeatStatuses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SeatStatuses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SeatStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "SeatStatuses",
                type: "timestamp(6) with time zone",
                precision: 6,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastUpdatedBy",
                table: "SeatStatuses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SeatStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }
    }
}
