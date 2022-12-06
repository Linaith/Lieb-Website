using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lieb.Migrations
{
    public partial class ReworkedGW2Build : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Heal",
                table: "GuildWars2Build",
                newName: "UseInRandomRaid");

            migrationBuilder.AddColumn<int>(
                name: "DamageType",
                table: "GuildWars2Build",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "GuildWars2Build",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamageType",
                table: "GuildWars2Build");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "GuildWars2Build");

            migrationBuilder.RenameColumn(
                name: "UseInRandomRaid",
                table: "GuildWars2Build",
                newName: "Heal");
        }
    }
}
