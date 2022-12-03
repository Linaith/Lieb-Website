using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lieb.Migrations
{
    public partial class RaidOwnerNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaidLog_LiebUser_UserId",
                table: "RaidLog");

            migrationBuilder.AlterColumn<ulong>(
                name: "RaidOwnerId",
                table: "RaidTemplate",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "RaidLog",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<ulong>(
                name: "RaidOwnerId",
                table: "Raid",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_RaidLog_LiebUser_UserId",
                table: "RaidLog",
                column: "UserId",
                principalTable: "LiebUser",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaidLog_LiebUser_UserId",
                table: "RaidLog");

            migrationBuilder.AlterColumn<ulong>(
                name: "RaidOwnerId",
                table: "RaidTemplate",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "RaidLog",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "RaidOwnerId",
                table: "Raid",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RaidLog_LiebUser_UserId",
                table: "RaidLog",
                column: "UserId",
                principalTable: "LiebUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
