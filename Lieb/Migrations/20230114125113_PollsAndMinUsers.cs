using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lieb.Migrations
{
    public partial class PollsAndMinUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MinUserDeadLine",
                table: "RaidTemplate",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MinUsers",
                table: "RaidTemplate",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MinUserDeadLineUTC",
                table: "Raid",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "MinUserPollId",
                table: "Raid",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinUsers",
                table: "Raid",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Poll",
                columns: table => new
                {
                    PollId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    AnswerType = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowCustomAnswer = table.Column<bool>(type: "INTEGER", nullable: false),
                    RaidId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poll", x => x.PollId);
                });

            migrationBuilder.CreateTable(
                name: "PollAnswer",
                columns: table => new
                {
                    PollAnswerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PollOptionId = table.Column<int>(type: "INTEGER", nullable: true),
                    Answer = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PollId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollAnswer", x => x.PollAnswerId);
                    table.ForeignKey(
                        name: "FK_PollAnswer_Poll_PollId",
                        column: x => x.PollId,
                        principalTable: "Poll",
                        principalColumn: "PollId");
                });

            migrationBuilder.CreateTable(
                name: "PollOption",
                columns: table => new
                {
                    PollOptionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PollId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollOption", x => x.PollOptionId);
                    table.ForeignKey(
                        name: "FK_PollOption_Poll_PollId",
                        column: x => x.PollId,
                        principalTable: "Poll",
                        principalColumn: "PollId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswer_PollId",
                table: "PollAnswer",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_PollOption_PollId",
                table: "PollOption",
                column: "PollId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollAnswer");

            migrationBuilder.DropTable(
                name: "PollOption");

            migrationBuilder.DropTable(
                name: "Poll");

            migrationBuilder.DropColumn(
                name: "MinUserDeadLine",
                table: "RaidTemplate");

            migrationBuilder.DropColumn(
                name: "MinUsers",
                table: "RaidTemplate");

            migrationBuilder.DropColumn(
                name: "MinUserDeadLineUTC",
                table: "Raid");

            migrationBuilder.DropColumn(
                name: "MinUserPollId",
                table: "Raid");

            migrationBuilder.DropColumn(
                name: "MinUsers",
                table: "Raid");
        }
    }
}
