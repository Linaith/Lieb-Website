using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lieb.Migrations
{
    public partial class AddRoleReminderTypeAndSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleType",
                table: "RaidReminder",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSubscription",
                table: "LiebUser",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleType",
                table: "RaidReminder");

            migrationBuilder.DropColumn(
                name: "ReminderSubscription",
                table: "LiebUser");
        }
    }
}
