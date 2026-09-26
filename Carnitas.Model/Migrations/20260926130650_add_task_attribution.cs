using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_task_attribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitiatorType",
                table: "QueuedTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InitiatorUserId",
                table: "QueuedTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitiatorType",
                table: "OperationRuns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InitiatorUserId",
                table: "OperationRuns",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueuedTasks_InitiatorUserId",
                table: "QueuedTasks",
                column: "InitiatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRuns_InitiatorUserId",
                table: "OperationRuns",
                column: "InitiatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRuns_AspNetUsers_InitiatorUserId",
                table: "OperationRuns",
                column: "InitiatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedTasks_AspNetUsers_InitiatorUserId",
                table: "QueuedTasks",
                column: "InitiatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationRuns_AspNetUsers_InitiatorUserId",
                table: "OperationRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_QueuedTasks_AspNetUsers_InitiatorUserId",
                table: "QueuedTasks");

            migrationBuilder.DropIndex(
                name: "IX_QueuedTasks_InitiatorUserId",
                table: "QueuedTasks");

            migrationBuilder.DropIndex(
                name: "IX_OperationRuns_InitiatorUserId",
                table: "OperationRuns");

            migrationBuilder.DropColumn(
                name: "InitiatorType",
                table: "QueuedTasks");

            migrationBuilder.DropColumn(
                name: "InitiatorUserId",
                table: "QueuedTasks");

            migrationBuilder.DropColumn(
                name: "InitiatorType",
                table: "OperationRuns");

            migrationBuilder.DropColumn(
                name: "InitiatorUserId",
                table: "OperationRuns");
        }
    }
}
