using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class gesturing_at_github_apps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RepositoryUrl",
                table: "Repository",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "GitHubAppId",
                table: "Repository",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Repository",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GitHubApps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    InstanceUrl = table.Column<string>(type: "text", nullable: true),
                    PrivateKey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubApps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Repository_GitHubAppId",
                table: "Repository",
                column: "GitHubAppId");

            migrationBuilder.AddForeignKey(
                name: "FK_Repository_GitHubApps_GitHubAppId",
                table: "Repository",
                column: "GitHubAppId",
                principalTable: "GitHubApps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Repository_GitHubApps_GitHubAppId",
                table: "Repository");

            migrationBuilder.DropTable(
                name: "GitHubApps");

            migrationBuilder.DropIndex(
                name: "IX_Repository_GitHubAppId",
                table: "Repository");

            migrationBuilder.DropColumn(
                name: "GitHubAppId",
                table: "Repository");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Repository");

            migrationBuilder.AlterColumn<string>(
                name: "RepositoryUrl",
                table: "Repository",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
