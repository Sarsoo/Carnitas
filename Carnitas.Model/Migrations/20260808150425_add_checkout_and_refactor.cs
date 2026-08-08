using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_checkout_and_refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RootModules_Repository_RepositoryId",
                table: "RootModules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RootModules",
                table: "RootModules");

            migrationBuilder.DropIndex(
                name: "IX_RootModules_RepositoryId",
                table: "RootModules");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "PlanRuns");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "PlanRuns");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ApplyRuns");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ApplyRuns");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "RootModules");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "RootModules");

            migrationBuilder.RenameTable(
                name: "RootModules",
                newName: "RootModule");

            migrationBuilder.AddColumn<string>(
                name: "RepositoryId",
                table: "PlanRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryId",
                table: "ApplyRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingBranch",
                table: "RootModule",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RootModule",
                table: "RootModule",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Checkouts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Branch = table.Column<string>(type: "text", nullable: true),
                    Commit = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RepositoryId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checkouts_Repository_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repository",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RepositoryId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Repository_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repository",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperationRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExitCode = table.Column<int>(type: "integer", nullable: true),
                    LogPath = table.Column<string>(type: "text", nullable: true),
                    GitReference = table.Column<string>(type: "text", nullable: true),
                    CommitSha = table.Column<string>(type: "text", nullable: true),
                    ModuleId = table.Column<string>(type: "text", nullable: false),
                    CheckoutId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationRuns_Checkouts_CheckoutId",
                        column: x => x.CheckoutId,
                        principalTable: "Checkouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OperationRuns_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "OperationRunLogEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OperationRunId = table.Column<string>(type: "text", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<JsonElement>(type: "jsonb", nullable: false),
                    OperationRunId1 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationRunLogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationRunLogEntries_OperationRuns_OperationRunId",
                        column: x => x.OperationRunId,
                        principalTable: "OperationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperationRunLogEntries_OperationRuns_OperationRunId1",
                        column: x => x.OperationRunId1,
                        principalTable: "OperationRuns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanRuns_RepositoryId",
                table: "PlanRuns",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplyRuns_RepositoryId",
                table: "ApplyRuns",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Checkouts_RepositoryId",
                table: "Checkouts",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InitRuns_RepositoryId",
                table: "InitRuns",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_RepositoryId",
                table: "Modules",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRunLogEntries_Level",
                table: "OperationRunLogEntries",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRunLogEntries_OperationRunId",
                table: "OperationRunLogEntries",
                column: "OperationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRunLogEntries_OperationRunId_Sequence",
                table: "OperationRunLogEntries",
                columns: new[] { "OperationRunId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_OperationRunLogEntries_OperationRunId1",
                table: "OperationRunLogEntries",
                column: "OperationRunId1");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRunLogEntries_Timestamp",
                table: "OperationRunLogEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRunLogEntries_Type",
                table: "OperationRunLogEntries",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRuns_CheckoutId",
                table: "OperationRuns",
                column: "CheckoutId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRuns_ModuleId",
                table: "OperationRuns",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplyRuns_OperationRuns_Id",
                table: "ApplyRuns",
                column: "Id",
                principalTable: "OperationRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplyRuns_Repository_RepositoryId",
                table: "ApplyRuns",
                column: "RepositoryId",
                principalTable: "Repository",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanRuns_OperationRuns_Id",
                table: "PlanRuns",
                column: "Id",
                principalTable: "OperationRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanRuns_Repository_RepositoryId",
                table: "PlanRuns",
                column: "RepositoryId",
                principalTable: "Repository",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RootModule_Modules_Id",
                table: "RootModule",
                column: "Id",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplyRuns_OperationRuns_Id",
                table: "ApplyRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplyRuns_Repository_RepositoryId",
                table: "ApplyRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanRuns_OperationRuns_Id",
                table: "PlanRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanRuns_Repository_RepositoryId",
                table: "PlanRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_RootModule_Modules_Id",
                table: "RootModule");

            migrationBuilder.DropTable(
                name: "InitRuns");

            migrationBuilder.DropTable(
                name: "OperationRunLogEntries");

            migrationBuilder.DropTable(
                name: "OperationRuns");

            migrationBuilder.DropTable(
                name: "Checkouts");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_PlanRuns_RepositoryId",
                table: "PlanRuns");

            migrationBuilder.DropIndex(
                name: "IX_ApplyRuns_RepositoryId",
                table: "ApplyRuns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RootModule",
                table: "RootModule");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "PlanRuns");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "ApplyRuns");

            migrationBuilder.DropColumn(
                name: "TrackingBranch",
                table: "RootModule");

            migrationBuilder.RenameTable(
                name: "RootModule",
                newName: "RootModules");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "PlanRuns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "PlanRuns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "ApplyRuns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "ApplyRuns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "RootModules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RepositoryId",
                table: "RootModules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RootModules",
                table: "RootModules",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RootModules_RepositoryId",
                table: "RootModules",
                column: "RepositoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_RootModules_Repository_RepositoryId",
                table: "RootModules",
                column: "RepositoryId",
                principalTable: "Repository",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
