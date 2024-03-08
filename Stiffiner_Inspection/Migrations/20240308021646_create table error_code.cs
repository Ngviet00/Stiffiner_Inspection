using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class createtableerror_code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "error_code");
        }
    }
}
