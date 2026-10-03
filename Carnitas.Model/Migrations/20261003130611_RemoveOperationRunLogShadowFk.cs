using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOperationRunLogShadowFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationRunLogEntries_OperationRuns_OperationRunId1",
                table: "OperationRunLogEntries");

            migrationBuilder.DropIndex(
                name: "IX_OperationRunLogEntries_OperationRunId1",
                table: "OperationRunLogEntries");

            migrationBuilder.DropColumn(
                name: "OperationRunId1",
                table: "OperationRunLogEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OperationRunId1",
                table: "OperationRunLogEntries",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationRunLogEntries_OperationRunId1",
                table: "OperationRunLogEntries",
                column: "OperationRunId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationRunLogEntries_OperationRuns_OperationRunId1",
                table: "OperationRunLogEntries",
                column: "OperationRunId1",
                principalTable: "OperationRuns",
                principalColumn: "Id");
        }
    }
}
