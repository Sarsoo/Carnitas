using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_source_discovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationRuns_Modules_ModuleId",
                table: "OperationRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_QueuedTasks_Modules_ModuleId",
                table: "QueuedTasks");

            migrationBuilder.DropIndex(
                name: "IX_Modules_RepositoryId",
                table: "Modules");

            migrationBuilder.AlterColumn<string>(
                name: "ModuleId",
                table: "QueuedTasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "RepositoryId",
                table: "QueuedTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModuleId",
                table: "OperationRuns",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "RelativePath",
                table: "Modules",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SourceDiscoveryRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RepositoryId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceDiscoveryRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceDiscoveryRuns_OperationRuns_Id",
                        column: x => x.Id,
                        principalTable: "OperationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SourceDiscoveryRuns_Repository_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repository",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueuedTasks_RepositoryId",
                table: "QueuedTasks",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_RepositoryId_RelativePath",
                table: "Modules",
                columns: new[] { "RepositoryId", "RelativePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceDiscoveryRuns_RepositoryId",
                table: "SourceDiscoveryRuns",
                column: "RepositoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRuns_Modules_ModuleId",
                table: "OperationRuns",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedTasks_Modules_ModuleId",
                table: "QueuedTasks",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedTasks_Repository_RepositoryId",
                table: "QueuedTasks",
                column: "RepositoryId",
                principalTable: "Repository",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationRuns_Modules_ModuleId",
                table: "OperationRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_QueuedTasks_Modules_ModuleId",
                table: "QueuedTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_QueuedTasks_Repository_RepositoryId",
                table: "QueuedTasks");

            migrationBuilder.DropTable(
                name: "SourceDiscoveryRuns");

            migrationBuilder.DropIndex(
                name: "IX_QueuedTasks_RepositoryId",
                table: "QueuedTasks");

            migrationBuilder.DropIndex(
                name: "IX_Modules_RepositoryId_RelativePath",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "QueuedTasks");

            migrationBuilder.DropColumn(
                name: "RelativePath",
                table: "Modules");

            migrationBuilder.AlterColumn<string>(
                name: "ModuleId",
                table: "QueuedTasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModuleId",
                table: "OperationRuns",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_RepositoryId",
                table: "Modules",
                column: "RepositoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRuns_Modules_ModuleId",
                table: "OperationRuns",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedTasks_Modules_ModuleId",
                table: "QueuedTasks",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
