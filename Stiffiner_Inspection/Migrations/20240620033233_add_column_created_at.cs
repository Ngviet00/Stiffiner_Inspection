using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class add_column_created_at : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "error_code");

            migrationBuilder.DropTable(
                name: "targets");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "images",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "errors",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "images");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "errors");

            migrationBuilder.CreateTable(
                name: "error_code",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    error_content = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_code", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "targets",
                columns: table => new
                {
                    target_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_date = table.Column<DateTime>(type: "DateTime", nullable: false),
                    target_qty = table.Column<int>(type: "int", nullable: false),
                    updated_date = table.Column<DateTime>(type: "DateTime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_targets", x => x.target_id);
                });

            migrationBuilder.InsertData(
                table: "error_code",
                columns: new[] { "id", "error_content" },
                values: new object[,]
                {
                    { 1L, "black dot" },
                    { 2L, "dirty" },
                    { 3L, "glue" },
                    { 4L, "ng sus position" },
                    { 5L, "ng hole" },
                    { 6L, "ng tape position" },
                    { 7L, "scratch" },
                    { 8L, "sus black dot" },
                    { 9L, "white dot" },
                    { 10L, "white line particle" },
                    { 11L, "dent-tray1" },
                    { 12L, "dent-tray2" },
                    { 13L, "deform" },
                    { 14L, "importinted" },
                    { 15L, "curl tape" },
                    { 16L, "curl sus" },
                    { 17L, "ng tape" }
                });
        }
    }
}
