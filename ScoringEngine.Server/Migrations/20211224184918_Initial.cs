using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScoringEngine.Server.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetitionSystems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReadmeText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionSystems", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ScoringItems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompetitionSystemID = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    ScriptType = table.Column<int>(type: "int", nullable: false),
                    Script = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoringItemType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringItems", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ScoringItems_CompetitionSystems_CompetitionSystemID",
                        column: x => x.CompetitionSystemID,
                        principalTable: "CompetitionSystems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredVirtualMachine",
                columns: table => new
                {
                    RegisteredVirtualMachineID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    SystemIdentifier = table.Column<int>(type: "int", nullable: false),
                    CompetitionSystemID = table.Column<int>(type: "int", nullable: true),
                    LastCheckIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConnectedNow = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredVirtualMachine", x => x.RegisteredVirtualMachineID);
                    table.ForeignKey(
                        name: "FK_RegisteredVirtualMachine_CompetitionSystems_CompetitionSystemID",
                        column: x => x.CompetitionSystemID,
                        principalTable: "CompetitionSystems",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegisteredVirtualMachine_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletedScoringItems",
                columns: table => new
                {
                    ScoringItemId = table.Column<int>(type: "int", nullable: false),
                    RegisteredVirtualMachineID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletionStatus = table.Column<int>(type: "int", nullable: false),
                    ApplicationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedScoringItems", x => new { x.ScoringItemId, x.RegisteredVirtualMachineID });
                    table.ForeignKey(
                        name: "FK_CompletedScoringItems_RegisteredVirtualMachine_RegisteredVirtualMachineID",
                        column: x => x.RegisteredVirtualMachineID,
                        principalTable: "RegisteredVirtualMachine",
                        principalColumn: "RegisteredVirtualMachineID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedScoringItems_ScoringItems_ScoringItemId",
                        column: x => x.ScoringItemId,
                        principalTable: "ScoringItems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedScoringItems_RegisteredVirtualMachineID",
                table: "CompletedScoringItems",
                column: "RegisteredVirtualMachineID");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVirtualMachine_CompetitionSystemID",
                table: "RegisteredVirtualMachine",
                column: "CompetitionSystemID");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVirtualMachine_TeamID",
                table: "RegisteredVirtualMachine",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringItems_CompetitionSystemID",
                table: "ScoringItems",
                column: "CompetitionSystemID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedScoringItems");

            migrationBuilder.DropTable(
                name: "RegisteredVirtualMachine");

            migrationBuilder.DropTable(
                name: "ScoringItems");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "CompetitionSystems");
        }
    }
}
