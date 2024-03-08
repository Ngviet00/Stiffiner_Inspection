using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class removecolumnerror_detechtion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "error_detection",
                table: "data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "error_detection",
                table: "data",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
