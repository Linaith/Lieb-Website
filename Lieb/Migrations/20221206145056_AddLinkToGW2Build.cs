using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lieb.Migrations
{
    public partial class AddLinkToGW2Build : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceLink",
                table: "GuildWars2Build",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceLink",
                table: "GuildWars2Build");
        }
    }
}
