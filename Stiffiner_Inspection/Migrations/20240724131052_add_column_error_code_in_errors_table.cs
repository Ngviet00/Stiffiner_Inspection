using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class add_column_error_code_in_errors_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "type_error",
                table: "errors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type_error",
                table: "errors");
        }
    }
}
