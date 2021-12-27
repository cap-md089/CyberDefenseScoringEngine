using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScoringEngine.Server.Migrations
{
    public partial class FixedKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletedScoringItems",
                table: "CompletedScoringItems");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "CompletedScoringItems",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletedScoringItems",
                table: "CompletedScoringItems",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedScoringItems_ScoringItemId",
                table: "CompletedScoringItems",
                column: "ScoringItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletedScoringItems",
                table: "CompletedScoringItems");

            migrationBuilder.DropIndex(
                name: "IX_CompletedScoringItems_ScoringItemId",
                table: "CompletedScoringItems");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "CompletedScoringItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletedScoringItems",
                table: "CompletedScoringItems",
                columns: new[] { "ScoringItemId", "RegisteredVirtualMachineID" });
        }
    }
}
