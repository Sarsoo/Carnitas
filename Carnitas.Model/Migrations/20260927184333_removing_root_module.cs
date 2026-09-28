using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class removing_root_module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RootModule");

            migrationBuilder.AddColumn<string>(
                name: "TrackingBranch",
                table: "Modules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Modules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackingBranch",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Modules");

            migrationBuilder.CreateTable(
                name: "RootModule",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TrackingBranch = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RootModule_Modules_Id",
                        column: x => x.Id,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
