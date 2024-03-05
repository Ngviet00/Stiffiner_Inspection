using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    /// <inheritdoc />
    public partial class create_tbl_data : Migration
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
                    result = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    error_code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    time = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    client_id = table.Column<int>(type: "int", nullable: true),
                    models = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    trays = table.Column<int>(type: "int", nullable: true),
                    sides = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    no = table.Column<int>(type: "int", nullable: true),
                    camera = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    error_detections = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    types = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    messages = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data");
        }
    }
}
