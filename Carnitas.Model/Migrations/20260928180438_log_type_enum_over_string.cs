using Carnitas.Job;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carnitas.Model.Migrations
{
    /// <inheritdoc />
    public partial class log_type_enum_over_string : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "OperationRunLogEntries");
            
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "OperationRunLogEntries",
                type: "integer",
                defaultValue: LogType.Json,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "OperationRunLogEntries");
            
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "OperationRunLogEntries",
                type: "text",
                defaultValue: "Json",
                nullable: false);
        }
    }
}
