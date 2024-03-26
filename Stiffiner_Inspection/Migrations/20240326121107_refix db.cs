using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class refixdb : Migration
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
                    tray = table.Column<int>(type: "int", nullable: false),
                    client_id = table.Column<int>(type: "int", nullable: true),
                    side = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    index = table.Column<int>(type: "int", nullable: true),
                    camera = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    result_area = table.Column<int>(type: "int", nullable: true),
                    result_line = table.Column<int>(type: "int", nullable: true),
                    target_id = table.Column<int>(type: "int", nullable: true)
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
                name: "targets",
                columns: table => new
                {
                    target_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    target_qty = table.Column<int>(type: "int", nullable: false),
                    created_date = table.Column<DateTime>(type: "DateTime", nullable: false),
                    updated_date = table.Column<DateTime>(type: "DateTime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_targets", x => x.target_id);
                });

            migrationBuilder.CreateTable(
                name: "errors",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    data_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_errors", x => x.id);
                    table.ForeignKey(
                        name: "FK_errors_data_data_id",
                        column: x => x.data_id,
                        principalTable: "data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    data_id = table.Column<long>(type: "bigint", nullable: false),
                    path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_images_data_data_id",
                        column: x => x.data_id,
                        principalTable: "data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_errors_data_id",
                table: "errors",
                column: "data_id");

            migrationBuilder.CreateIndex(
                name: "IX_images_data_id",
                table: "images",
                column: "data_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "error_code");

            migrationBuilder.DropTable(
                name: "errors");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropTable(
                name: "targets");

            migrationBuilder.DropTable(
                name: "data");
        }
    }
}
