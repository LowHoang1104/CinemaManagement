using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddShowTimeFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "ShowTimes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Format",
                table: "ShowTimes");
        }
    }
}
