using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class addcolumnclient_idtotableimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "targets",
                keyColumn: "target_id",
                keyValue: 1L);

            migrationBuilder.AddColumn<int>(
                name: "client_id",
                table: "images",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_id",
                table: "images");

            migrationBuilder.InsertData(
                table: "targets",
                columns: new[] { "target_id", "created_date", "target_qty", "updated_date" },
                values: new object[] { 1L, new DateTime(2024, 3, 26, 19, 16, 3, 790, DateTimeKind.Local).AddTicks(3403), 2000, new DateTime(2024, 3, 26, 19, 16, 3, 790, DateTimeKind.Local).AddTicks(3412) });
        }
    }
}
