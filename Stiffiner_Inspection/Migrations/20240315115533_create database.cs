using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class createdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    model = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    tray = table.Column<int>(type: "int", nullable: true),
                    client_id = table.Column<int>(type: "int", nullable: true),
                    side = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    index = table.Column<int>(type: "int", nullable: true),
                    camera = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    result = table.Column<int>(type: "int", nullable: true),
                    error_code = table.Column<int>(type: "int", nullable: true),
                    image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    time_start = table.Column<DateTime>(type: "datetime2", nullable: true),
                    time_end = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data", x => x.id);
                });

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
                name: "time_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    time = table.Column<DateTime>(type: "datetime2", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    message = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_logs", x => x.id);
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data");

            migrationBuilder.DropTable(
                name: "error_code");

            migrationBuilder.DropTable(
                name: "time_logs");
        }
    }
}
