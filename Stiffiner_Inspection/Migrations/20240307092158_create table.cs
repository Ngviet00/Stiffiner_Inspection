using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class createtable : Migration
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
                    error_detection = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    time_start = table.Column<DateTime>(type: "datetime2", nullable: true),
                    time_end = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data", x => x.id);
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data");

            migrationBuilder.DropTable(
                name: "time_logs");
        }
    }
}
