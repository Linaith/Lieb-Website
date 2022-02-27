using Lieb.Models;
using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class DbInitializer
    {
        public static void Initialize(LiebContext context)
        {
            //add special Roles
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == Constants.Roles.Admin) == null)
            {
                context.LiebRoles.Add(new LiebRole() { RoleName = Constants.Roles.Admin, IsSystemRole = true, Level = Constants.RoleLevels.AdminLevel, LevelToAssign = Constants.RoleLevels.AdminLevel });
            }
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == Constants.Roles.GuildLead) == null)
            {
                context.LiebRoles.Add(new LiebRole() { RoleName = Constants.Roles.GuildLead, IsSystemRole = true, Level = Constants.RoleLevels.GuildLeadLevel, LevelToAssign = Constants.RoleLevels.AdminLevel });
            }
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == Constants.Roles.RaidLead) == null)
            {
                context.LiebRoles.Add(new LiebRole() { RoleName = Constants.Roles.RaidLead, IsSystemRole = true, Level = Constants.RoleLevels.RaidLeadLevel, LevelToAssign = Constants.RoleLevels.GuildLeadLevel });
            }
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == Constants.Roles.User) == null)
            {
                context.LiebRoles.Add(new LiebRole() { RoleName = Constants.Roles.User, IsSystemRole = true, Level = Constants.RoleLevels.UserLevel, LevelToAssign = Constants.RoleLevels.AdminLevel + 1  });
            }
            context.SaveChanges();

            // Look for any LiebUsers.
            if (context.LiebUsers.Any())
            {
                return;   // DB has been seeded
            }

            GuildWars2Account linaith = new GuildWars2Account() { AccountName = "Linaith.2375" };
            GuildWars2Account sarah = new GuildWars2Account() { AccountName = "Sarah.3984" };
            GuildWars2Account hierpiepts = new GuildWars2Account() { AccountName = "hierpiepts.5241" };
            GuildWars2Account bloodseeker = new GuildWars2Account() { AccountName = "Bloodseeker.2043" };
            var users = new LiebUser[]
            {
                new LiebUser{DiscordUserId=194863625477816321, Name="Sarah", Birthday=DateTime.Parse("1992-01-15"), GuildWars2Accounts = new List<GuildWars2Account>(){ linaith, sarah} },
#if DEBUG
                new LiebUser{DiscordUserId=1, Name="Lisa", GuildWars2Accounts = new List<GuildWars2Account>(){ hierpiepts}},
                new LiebUser{DiscordUserId=2, Name="Simon", GuildWars2Accounts = new List<GuildWars2Account>(){ bloodseeker}}
#endif
            };

            context.LiebUsers.AddRange(users);
            context.SaveChanges();


            int AdminRoleId = context.LiebRoles.FirstOrDefault(x => x.RoleName == Constants.Roles.Admin).LiebRoleId;
            int GuildLeadRoleId = context.LiebRoles.FirstOrDefault(x => x.RoleName == Constants.Roles.GuildLead).LiebRoleId;
            int RaidLeadRoleId = context.LiebRoles.FirstOrDefault(x => x.RoleName == Constants.Roles.RaidLead).LiebRoleId;
            int UserRoleId = context.LiebRoles.FirstOrDefault(x => x.RoleName == Constants.Roles.User).LiebRoleId;

            var assignments = new RoleAssignment[]
            {
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = AdminRoleId },
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = GuildLeadRoleId },
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = RaidLeadRoleId },
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = UserRoleId }
#if DEBUG
                ,new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = AdminRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = GuildLeadRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = RaidLeadRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = UserRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = GuildLeadRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = RaidLeadRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = UserRoleId }
#endif
            };

            context.RoleAssignments.AddRange(assignments);
            context.SaveChanges();
#if DEBUG
            PlannedRaidRole ele = new PlannedRaidRole()
            {
                Description = "Beste",
                Name = "Heal Ele",
                Spots = 2
            };
            PlannedRaidRole scourge = new PlannedRaidRole()
            {
                Description = "WupWup",
                Name = "Scourge",
                Spots = 8
            };

            Raid raid = new Raid()
            {
                Title = "Testraid",
                Description = "This is a test raid\nwith multiple lines?",
                Guild = "LIEB",
                Organizer = "Sarah",
                RaidType = RaidType.RandomClasses,
                Date = DateTime.Now.Date,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                VoiceChat = "ts.lieb.games",
                Roles = new [] { ele, scourge}
            };
            context.Raids.Add(raid);
            context.SaveChanges();

            var signUps = new RaidSignUp[]
            {
                new RaidSignUp{GuildWars2AccountId = linaith.GuildWars2AccountId, LiebUserId = users[0].LiebUserId, PlannedRaidRoleId = ele.PlannedRaidRoleId, RaidId = raid.RaidId, SignUpType = SignUpType.SignedUp },
                new RaidSignUp{GuildWars2AccountId = hierpiepts.GuildWars2AccountId, LiebUserId = users[1].LiebUserId, PlannedRaidRoleId = scourge.PlannedRaidRoleId, RaidId = raid.RaidId, SignUpType = SignUpType.SignedUp },
                new RaidSignUp{GuildWars2AccountId = bloodseeker.GuildWars2AccountId, LiebUserId = users[2].LiebUserId, PlannedRaidRoleId = scourge.PlannedRaidRoleId, RaidId = raid.RaidId, SignUpType = SignUpType.Maybe }
            };

            context.RaidSignUps.AddRange(signUps);
            context.SaveChanges();
#endif
            }
    }
}
