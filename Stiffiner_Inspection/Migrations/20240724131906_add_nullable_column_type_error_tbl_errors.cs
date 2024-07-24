using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class add_nullable_column_type_error_tbl_errors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "type_error",
                table: "errors",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "type_error",
                table: "errors",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
