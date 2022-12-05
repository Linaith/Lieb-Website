using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lieb.Migrations
{
    public partial class RemovedRaidLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaidLog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RaidLog",
                columns: table => new
                {
                    RaidLogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaidId = table.Column<int>(type: "INTEGER", nullable: true),
                    RaidTemplateId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    LogEntry = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidLog", x => x.RaidLogId);
                    table.ForeignKey(
                        name: "FK_RaidLog_LiebUser_UserId",
                        column: x => x.UserId,
                        principalTable: "LiebUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RaidLog_Raid_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raid",
                        principalColumn: "RaidId");
                    table.ForeignKey(
                        name: "FK_RaidLog_RaidTemplate_RaidTemplateId",
                        column: x => x.RaidTemplateId,
                        principalTable: "RaidTemplate",
                        principalColumn: "RaidTemplateId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RaidLog_RaidId",
                table: "RaidLog",
                column: "RaidId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidLog_RaidTemplateId",
                table: "RaidLog",
                column: "RaidTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidLog_UserId",
                table: "RaidLog",
                column: "UserId");
        }
    }
}
