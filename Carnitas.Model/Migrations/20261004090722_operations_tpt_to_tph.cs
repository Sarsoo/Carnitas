using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class operations_tpt_to_tph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplyRuns");

            migrationBuilder.DropTable(
                name: "InitRuns");

            migrationBuilder.DropTable(
                name: "PlanRuns");

            migrationBuilder.DropTable(
                name: "SourceDiscoveryRuns");

            migrationBuilder.AddColumn<string>(
                name: "ApplyRun_RepositoryId",
                table: "OperationRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanRun_RepositoryId",
                table: "OperationRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryId",
                table: "OperationRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDiscoveryRun_RepositoryId",
                table: "OperationRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "operation_type",
                table: "OperationRuns",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.CreateIndex(
                name: "IX_OperationRuns_ApplyRun_RepositoryId",
                table: "OperationRuns",
                column: "ApplyRun_RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRuns_PlanRun_RepositoryId",
                table: "OperationRuns",
                column: "PlanRun_RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRuns_RepositoryId",
                table: "OperationRuns",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRuns_SourceDiscoveryRun_RepositoryId",
                table: "OperationRuns",
                column: "SourceDiscoveryRun_RepositoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRuns_Repository_ApplyRun_RepositoryId",
                table: "OperationRuns",
                column: "ApplyRun_RepositoryId",
                principalTable: "Repository",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRuns_Repository_PlanRun_RepositoryId",
                table: "OperationRuns",
                column: "PlanRun_RepositoryId",
                principalTable: "Repository",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRuns_Repository_RepositoryId",
                table: "OperationRuns",
                column: "RepositoryId",
                principalTable: "Repository",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRuns_Repository_SourceDiscoveryRun_RepositoryId",
                table: "OperationRuns",
                column: "SourceDiscoveryRun_RepositoryId",
                principalTable: "Repository",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationRuns_Repository_ApplyRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_OperationRuns_Repository_PlanRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_OperationRuns_Repository_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_OperationRuns_Repository_SourceDiscoveryRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropIndex(
                name: "IX_OperationRuns_ApplyRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropIndex(
                name: "IX_OperationRuns_PlanRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropIndex(
                name: "IX_OperationRuns_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropIndex(
                name: "IX_OperationRuns_SourceDiscoveryRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropColumn(
                name: "ApplyRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropColumn(
                name: "PlanRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropColumn(
                name: "SourceDiscoveryRun_RepositoryId",
                table: "OperationRuns");

            migrationBuilder.DropColumn(
                name: "operation_type",
                table: "OperationRuns");

            migrationBuilder.CreateTable(
                name: "ApplyRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RepositoryId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplyRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplyRuns_OperationRuns_Id",
                        column: x => x.Id,
                        principalTable: "OperationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplyRuns_Repository_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repository",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InitRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RepositoryId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InitRuns_OperationRuns_Id",
                        column: x => x.Id,
                        principalTable: "OperationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InitRuns_Repository_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repository",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RepositoryId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanRuns_OperationRuns_Id",
                        column: x => x.Id,
                        principalTable: "OperationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanRuns_Repository_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repository",
                        principalColumn: "Id");
                });

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
                name: "IX_ApplyRuns_RepositoryId",
                table: "ApplyRuns",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InitRuns_RepositoryId",
                table: "InitRuns",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanRuns_RepositoryId",
                table: "PlanRuns",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceDiscoveryRuns_RepositoryId",
                table: "SourceDiscoveryRuns",
                column: "RepositoryId");
        }
    }
}
