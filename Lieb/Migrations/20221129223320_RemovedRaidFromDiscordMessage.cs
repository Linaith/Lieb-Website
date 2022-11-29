using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lieb.Migrations
{
    public partial class RemovedRaidFromDiscordMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordRaidMessage_Raid_RaidId",
                table: "DiscordRaidMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_RaidReminder_Raid_RaidId",
                table: "RaidReminder");

            migrationBuilder.AlterColumn<int>(
                name: "RaidId",
                table: "RaidReminder",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "RaidId",
                table: "DiscordRaidMessage",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordRaidMessage_Raid_RaidId",
                table: "DiscordRaidMessage",
                column: "RaidId",
                principalTable: "Raid",
                principalColumn: "RaidId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaidReminder_Raid_RaidId",
                table: "RaidReminder",
                column: "RaidId",
                principalTable: "Raid",
                principalColumn: "RaidId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordRaidMessage_Raid_RaidId",
                table: "DiscordRaidMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_RaidReminder_Raid_RaidId",
                table: "RaidReminder");

            migrationBuilder.AlterColumn<int>(
                name: "RaidId",
                table: "RaidReminder",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RaidId",
                table: "DiscordRaidMessage",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordRaidMessage_Raid_RaidId",
                table: "DiscordRaidMessage",
                column: "RaidId",
                principalTable: "Raid",
                principalColumn: "RaidId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaidReminder_Raid_RaidId",
                table: "RaidReminder",
                column: "RaidId",
                principalTable: "Raid",
                principalColumn: "RaidId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
