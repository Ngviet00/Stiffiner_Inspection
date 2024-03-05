using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class edit_column_table_data_and_create_tbl_time_log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "messages",
                table: "data");

            migrationBuilder.DropColumn(
                name: "models",
                table: "data");

            migrationBuilder.RenameColumn(
                name: "error_detections",
                table: "data",
                newName: "error_detection");

            migrationBuilder.RenameColumn(
                name: "types",
                table: "data",
                newName: "side");

            migrationBuilder.RenameColumn(
                name: "trays",
                table: "data",
                newName: "tray");

            migrationBuilder.RenameColumn(
                name: "sides",
                table: "data",
                newName: "model");

            migrationBuilder.AlterColumn<DateTime>(
                name: "time",
                table: "data",
                type: "datetime2",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "error_detection",
                table: "data",
                newName: "error_detections");

            migrationBuilder.RenameColumn(
                name: "tray",
                table: "data",
                newName: "trays");

            migrationBuilder.RenameColumn(
                name: "side",
                table: "data",
                newName: "types");

            migrationBuilder.RenameColumn(
                name: "model",
                table: "data",
                newName: "sides");

            migrationBuilder.AlterColumn<string>(
                name: "time",
                table: "data",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "messages",
                table: "data",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "models",
                table: "data",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
