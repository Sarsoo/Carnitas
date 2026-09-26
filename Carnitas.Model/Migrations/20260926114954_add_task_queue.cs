using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_task_queue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QueuedTaskId",
                table: "OperationRuns",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QueuedTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RepoUrl = table.Column<string>(type: "text", nullable: false),
                    ModulePath = table.Column<string>(type: "text", nullable: false),
                    ModuleId = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockedBy = table.Column<string>(type: "text", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueuedTasks_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueuedTaskOperations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    QueuedTaskId = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedTaskOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueuedTaskOperations_QueuedTasks_QueuedTaskId",
                        column: x => x.QueuedTaskId,
                        principalTable: "QueuedTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationRuns_QueuedTaskId",
                table: "OperationRuns",
                column: "QueuedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedTaskOperations_QueuedTaskId_Kind",
                table: "QueuedTaskOperations",
                columns: new[] { "QueuedTaskId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueuedTaskOperations_QueuedTaskId_Sequence",
                table: "QueuedTaskOperations",
                columns: new[] { "QueuedTaskId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueuedTasks_ModuleId",
                table: "QueuedTasks",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedTasks_State_LockedUntil",
                table: "QueuedTasks",
                columns: new[] { "State", "LockedUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_QueuedTasks_State_ScheduledAt_Priority_CreatedAt",
                table: "QueuedTasks",
                columns: new[] { "State", "ScheduledAt", "Priority", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRuns_QueuedTasks_QueuedTaskId",
                table: "OperationRuns",
                column: "QueuedTaskId",
                principalTable: "QueuedTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationRuns_QueuedTasks_QueuedTaskId",
                table: "OperationRuns");

            migrationBuilder.DropTable(
                name: "QueuedTaskOperations");

            migrationBuilder.DropTable(
                name: "QueuedTasks");

            migrationBuilder.DropIndex(
                name: "IX_OperationRuns_QueuedTaskId",
                table: "OperationRuns");

            migrationBuilder.DropColumn(
                name: "QueuedTaskId",
                table: "OperationRuns");
        }
    }
}
