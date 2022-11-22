using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lieb.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordSettings",
                columns: table => new
                {
                    DiscordSettingsId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordLogChannel = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChangeUserNames = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordSettings", x => x.DiscordSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "GuildWars2Build",
                columns: table => new
                {
                    GuildWars2BuildId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildName = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Might = table.Column<short>(type: "INTEGER", nullable: false),
                    Quickness = table.Column<short>(type: "INTEGER", nullable: false),
                    Alacrity = table.Column<short>(type: "INTEGER", nullable: false),
                    Heal = table.Column<short>(type: "INTEGER", nullable: false),
                    Class = table.Column<int>(type: "INTEGER", nullable: false),
                    EliteSpecialization = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildWars2Build", x => x.GuildWars2BuildId);
                });

            migrationBuilder.CreateTable(
                name: "LiebRole",
                columns: table => new
                {
                    LiebRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    LevelToAssign = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiebRole", x => x.LiebRoleId);
                });

            migrationBuilder.CreateTable(
                name: "LiebUser",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Pronouns = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Birthday = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BannedUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MainGW2Account = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiebUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Raid",
                columns: table => new
                {
                    RaidId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartTimeUTC = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndTimeUTC = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FreeForAllTimeUTC = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Organizer = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Guild = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    VoiceChat = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RaidType = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredRole = table.Column<string>(type: "TEXT", nullable: false),
                    MoveFlexUsers = table.Column<bool>(type: "INTEGER", nullable: false),
                    RaidOwnerId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Raid", x => x.RaidId);
                });

            migrationBuilder.CreateTable(
                name: "RaidTemplate",
                columns: table => new
                {
                    RaidTemplateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FreeForAllTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", nullable: false),
                    Interval = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateDaysBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Organizer = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Guild = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    VoiceChat = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RaidType = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredRole = table.Column<string>(type: "TEXT", nullable: false),
                    MoveFlexUsers = table.Column<bool>(type: "INTEGER", nullable: false),
                    RaidOwnerId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidTemplate", x => x.RaidTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "GuildWars2Account",
                columns: table => new
                {
                    GuildWars2AccountId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", nullable: false),
                    LiebUserId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildWars2Account", x => x.GuildWars2AccountId);
                    table.ForeignKey(
                        name: "FK_GuildWars2Account_LiebUser_LiebUserId",
                        column: x => x.LiebUserId,
                        principalTable: "LiebUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleAssignment",
                columns: table => new
                {
                    RoleAssignmentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LiebRoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    LiebUserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignment", x => x.RoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_RoleAssignment_LiebRole_LiebRoleId",
                        column: x => x.LiebRoleId,
                        principalTable: "LiebRole",
                        principalColumn: "LiebRoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleAssignment_LiebUser_LiebUserId",
                        column: x => x.LiebUserId,
                        principalTable: "LiebUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordRaidMessage",
                columns: table => new
                {
                    DiscordRaidMessageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaidId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscordMessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordGuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RaidTemplateId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordRaidMessage", x => x.DiscordRaidMessageId);
                    table.ForeignKey(
                        name: "FK_DiscordRaidMessage_Raid_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raid",
                        principalColumn: "RaidId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordRaidMessage_RaidTemplate_RaidTemplateId",
                        column: x => x.RaidTemplateId,
                        principalTable: "RaidTemplate",
                        principalColumn: "RaidTemplateId");
                });

            migrationBuilder.CreateTable(
                name: "RaidLog",
                columns: table => new
                {
                    RaidLogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RaidId = table.Column<int>(type: "INTEGER", nullable: true),
                    RaidTemplateId = table.Column<int>(type: "INTEGER", nullable: true),
                    LogEntry = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidLog", x => x.RaidLogId);
                    table.ForeignKey(
                        name: "FK_RaidLog_LiebUser_UserId",
                        column: x => x.UserId,
                        principalTable: "LiebUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "RaidReminder",
                columns: table => new
                {
                    RaidReminderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeType = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ReminderTimeUTC = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DiscordServerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Sent = table.Column<bool>(type: "INTEGER", nullable: false),
                    RaidId = table.Column<int>(type: "INTEGER", nullable: false),
                    RaidTemplateId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidReminder", x => x.RaidReminderId);
                    table.ForeignKey(
                        name: "FK_RaidReminder_Raid_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raid",
                        principalColumn: "RaidId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaidReminder_RaidTemplate_RaidTemplateId",
                        column: x => x.RaidTemplateId,
                        principalTable: "RaidTemplate",
                        principalColumn: "RaidTemplateId");
                });

            migrationBuilder.CreateTable(
                name: "RaidRole",
                columns: table => new
                {
                    RaidRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Spots = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsRandomSignUpRole = table.Column<bool>(type: "INTEGER", nullable: false),
                    RaidId = table.Column<int>(type: "INTEGER", nullable: true),
                    RaidTemplateId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidRole", x => x.RaidRoleId);
                    table.ForeignKey(
                        name: "FK_RaidRole_Raid_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raid",
                        principalColumn: "RaidId");
                    table.ForeignKey(
                        name: "FK_RaidRole_RaidTemplate_RaidTemplateId",
                        column: x => x.RaidTemplateId,
                        principalTable: "RaidTemplate",
                        principalColumn: "RaidTemplateId");
                });

            migrationBuilder.CreateTable(
                name: "Equipped",
                columns: table => new
                {
                    EquippedId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CanTank = table.Column<bool>(type: "INTEGER", nullable: false),
                    GuildWars2AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuildWars2BuildId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipped", x => x.EquippedId);
                    table.ForeignKey(
                        name: "FK_Equipped_GuildWars2Account_GuildWars2AccountId",
                        column: x => x.GuildWars2AccountId,
                        principalTable: "GuildWars2Account",
                        principalColumn: "GuildWars2AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Equipped_GuildWars2Build_GuildWars2BuildId",
                        column: x => x.GuildWars2BuildId,
                        principalTable: "GuildWars2Build",
                        principalColumn: "GuildWars2BuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaidSignUp",
                columns: table => new
                {
                    RaidSignUpId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaidId = table.Column<int>(type: "INTEGER", nullable: false),
                    LiebUserId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    GuildWars2AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    RaidRoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalUserName = table.Column<string>(type: "TEXT", nullable: false),
                    SignUpType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidSignUp", x => x.RaidSignUpId);
                    table.ForeignKey(
                        name: "FK_RaidSignUp_GuildWars2Account_GuildWars2AccountId",
                        column: x => x.GuildWars2AccountId,
                        principalTable: "GuildWars2Account",
                        principalColumn: "GuildWars2AccountId");
                    table.ForeignKey(
                        name: "FK_RaidSignUp_LiebUser_LiebUserId",
                        column: x => x.LiebUserId,
                        principalTable: "LiebUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RaidSignUp_Raid_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raid",
                        principalColumn: "RaidId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaidSignUp_RaidRole_RaidRoleId",
                        column: x => x.RaidRoleId,
                        principalTable: "RaidRole",
                        principalColumn: "RaidRoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordRaidMessage_RaidId",
                table: "DiscordRaidMessage",
                column: "RaidId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordRaidMessage_RaidTemplateId",
                table: "DiscordRaidMessage",
                column: "RaidTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipped_GuildWars2AccountId",
                table: "Equipped",
                column: "GuildWars2AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipped_GuildWars2BuildId",
                table: "Equipped",
                column: "GuildWars2BuildId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildWars2Account_LiebUserId",
                table: "GuildWars2Account",
                column: "LiebUserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_RaidReminder_RaidId",
                table: "RaidReminder",
                column: "RaidId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidReminder_RaidTemplateId",
                table: "RaidReminder",
                column: "RaidTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidRole_RaidId",
                table: "RaidRole",
                column: "RaidId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidRole_RaidTemplateId",
                table: "RaidRole",
                column: "RaidTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidSignUp_GuildWars2AccountId",
                table: "RaidSignUp",
                column: "GuildWars2AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidSignUp_LiebUserId",
                table: "RaidSignUp",
                column: "LiebUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidSignUp_RaidId",
                table: "RaidSignUp",
                column: "RaidId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidSignUp_RaidRoleId",
                table: "RaidSignUp",
                column: "RaidRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignment_LiebRoleId",
                table: "RoleAssignment",
                column: "LiebRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignment_LiebUserId",
                table: "RoleAssignment",
                column: "LiebUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordRaidMessage");

            migrationBuilder.DropTable(
                name: "DiscordSettings");

            migrationBuilder.DropTable(
                name: "Equipped");

            migrationBuilder.DropTable(
                name: "RaidLog");

            migrationBuilder.DropTable(
                name: "RaidReminder");

            migrationBuilder.DropTable(
                name: "RaidSignUp");

            migrationBuilder.DropTable(
                name: "RoleAssignment");

            migrationBuilder.DropTable(
                name: "GuildWars2Build");

            migrationBuilder.DropTable(
                name: "GuildWars2Account");

            migrationBuilder.DropTable(
                name: "RaidRole");

            migrationBuilder.DropTable(
                name: "LiebRole");

            migrationBuilder.DropTable(
                name: "LiebUser");

            migrationBuilder.DropTable(
                name: "Raid");

            migrationBuilder.DropTable(
                name: "RaidTemplate");
        }
    }
}
