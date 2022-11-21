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
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == Constants.Roles.Admin.Name) == null)
            {
                context.LiebRoles.Add(new LiebRole() { RoleName = Constants.Roles.Admin.Name, Type = RoleType.SystemRole, Level = Constants.Roles.Admin.PowerLevel, LevelToAssign = Constants.Roles.Admin.PowerLevel });
            }
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == Constants.Roles.Moderator.Name) == null)
            {
                context.LiebRoles.Add(new LiebRole() { RoleName = Constants.Roles.Moderator.Name, Type = RoleType.SystemRole, Level = Constants.Roles.Moderator.PowerLevel, LevelToAssign = Constants.Roles.Admin.PowerLevel });
            }
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == Constants.Roles.RaidLead.Name) == null)
            {
                context.LiebRoles.Add(new LiebRole() { RoleName = Constants.Roles.RaidLead.Name, Type = RoleType.SystemRole, Level = Constants.Roles.RaidLead.PowerLevel, LevelToAssign = Constants.Roles.Moderator.PowerLevel });
            }
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == Constants.Roles.User.Name) == null)
            {
                context.LiebRoles.Add(new LiebRole() { RoleName = Constants.Roles.User.Name, Type = RoleType.SystemRole, Level = Constants.Roles.User.PowerLevel, LevelToAssign = Constants.Roles.Admin.PowerLevel + 1  });
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
                new LiebUser{Id=0, Name="Sarah", Birthday=DateTime.Parse("1992-01-15"), GuildWars2Accounts = new List<GuildWars2Account>(){ linaith, sarah} },
                //new LiebUser{Id=194863625477816321, Name="Sarah", Birthday=DateTime.Parse("1992-01-15"), GuildWars2Accounts = new List<GuildWars2Account>(){ linaith, sarah} },
#if DEBUG
                //new LiebUser{Id=194455125769715713, Name="Lisa", GuildWars2Accounts = new List<GuildWars2Account>(){ hierpiepts}},
                new LiebUser{Id=1, Name="Lisa", GuildWars2Accounts = new List<GuildWars2Account>(){ hierpiepts}},
                new LiebUser{Id=2, Name="Simon", GuildWars2Accounts = new List<GuildWars2Account>(){ bloodseeker}}
#endif
            };

            context.LiebUsers.AddRange(users);
            context.SaveChanges();


            int AdminRoleId = context.LiebRoles.FirstOrDefault(x => x.RoleName == Constants.Roles.Admin.Name).LiebRoleId;
            int GuildLeadRoleId = context.LiebRoles.FirstOrDefault(x => x.RoleName == Constants.Roles.Moderator.Name).LiebRoleId;
            int RaidLeadRoleId = context.LiebRoles.FirstOrDefault(x => x.RoleName == Constants.Roles.RaidLead.Name).LiebRoleId;
            int UserRoleId = context.LiebRoles.FirstOrDefault(x => x.RoleName == Constants.Roles.User.Name).LiebRoleId;

            var assignments = new RoleAssignment[]
            {
                new RoleAssignment{LiebUserId = users[0].Id, LiebRoleId = AdminRoleId },
                new RoleAssignment{LiebUserId = users[0].Id, LiebRoleId = GuildLeadRoleId },
                new RoleAssignment{LiebUserId = users[0].Id, LiebRoleId = RaidLeadRoleId },
                new RoleAssignment{LiebUserId = users[0].Id, LiebRoleId = UserRoleId }
#if DEBUG
                ,new RoleAssignment{LiebUserId = users[1].Id, LiebRoleId = AdminRoleId },
                new RoleAssignment{LiebUserId = users[1].Id, LiebRoleId = GuildLeadRoleId },
                new RoleAssignment{LiebUserId = users[1].Id, LiebRoleId = RaidLeadRoleId },
                new RoleAssignment{LiebUserId = users[1].Id, LiebRoleId = UserRoleId },
                new RoleAssignment{LiebUserId = users[2].Id, LiebRoleId = GuildLeadRoleId },
                new RoleAssignment{LiebUserId = users[2].Id, LiebRoleId = RaidLeadRoleId },
                new RoleAssignment{LiebUserId = users[2].Id, LiebRoleId = UserRoleId }
#endif
            };

            context.RoleAssignments.AddRange(assignments);
            context.SaveChanges();
#if DEBUG
            RaidRole ele = new RaidRole()
            {
                Description = "Beste",
                Name = "Heal Ele",
                Spots = 2
            };
            RaidRole scourge = new RaidRole()
            {
                Description = "WupWup",
                Name = "Scourge",
                Spots = 8
            };
            RaidRole randomRole = new RaidRole()
            {
                Spots = 10,
                Name = "Random",
                Description = RaidType.RandomWithBoons.ToString(),
                IsRandomSignUpRole = true
            };
            RaidRole flexTest1 = new RaidRole()
            {
                Description = "flexTest1",
                Name = "flexTest1",
                Spots = 1
            };
            RaidRole flexTest2 = new RaidRole()
            {
                Description = "flexTest2",
                Name = "flexTest2",
                Spots = 1
            };
            RaidRole flexTest3 = new RaidRole()
            {
                Description = "flexTest3",
                Name = "flexTest3",
                Spots = 1
            };

            Raid raid = new Raid()
            {
                Title = "Testraid",
                Description = "This is a test raid\nwith multiple lines?",
                Guild = "LIEB",
                Organizer = "Sarah",
                RaidType = RaidType.Planned,
                StartTimeUTC = DateTime.UtcNow,
                EndTimeUTC = DateTime.UtcNow.AddHours(2),
                FreeForAllTimeUTC = DateTime.UtcNow.AddHours(-2),
                VoiceChat = "ts.lieb.games",
                Roles = new [] { randomRole, ele, scourge, flexTest1, flexTest2, flexTest3 }
            };
            context.Raids.Add(raid);
            context.SaveChanges();

            DateTime templateStartTime = DateTime.UtcNow.AddDays(-7).AddMinutes(-117);
            RaidTemplate template = new RaidTemplate()
            {
                Title = "Testraid",
                Description = "This is a test raid\nwith multiple lines?",
                Guild = "LIEB",
                Organizer = "Sarah",
                RaidType = RaidType.RandomWithBoons,
                StartTime = templateStartTime,
                EndTime = templateStartTime.AddHours(2),
                FreeForAllTime = templateStartTime.AddHours(-2),
                VoiceChat = "ts.lieb.games",
                Interval = 7,
                CreateDaysBefore = 7,
                TimeZone = "Europe/Vienna",
                Roles = new[] { new RaidRole(){
                        Description = "WupWup",
                        Name = "Ups",
                        Spots = 10
                    } 
                }
            };
            context.RaidTemplates.Add(template);
            context.SaveChanges();

            var signUps = new RaidSignUp[]
            {
                new RaidSignUp(raid.RaidId, users[0].Id, linaith.GuildWars2AccountId, ele.RaidRoleId, SignUpType.SignedUp),
                new RaidSignUp(raid.RaidId, users[1].Id, hierpiepts.GuildWars2AccountId, flexTest1.RaidRoleId, SignUpType.SignedUp),
                new RaidSignUp(raid.RaidId, users[2].Id, bloodseeker.GuildWars2AccountId, flexTest2.RaidRoleId, SignUpType.SignedUp),
                new RaidSignUp(raid.RaidId, users[1].Id, hierpiepts.GuildWars2AccountId, flexTest2.RaidRoleId, SignUpType.Flex),
                new RaidSignUp(raid.RaidId, users[2].Id, bloodseeker.GuildWars2AccountId, flexTest3.RaidRoleId, SignUpType.Flex)
            };

            context.RaidSignUps.AddRange(signUps);
            context.SaveChanges();

            GuildWars2Build healTempest = new GuildWars2Build() { BuildName = "HealTempest", Class = GuildWars2Class.Elementalist, EliteSpecialization = EliteSpecialization.Tempest, Heal = 5, Might = 5 };
            GuildWars2Build condiScourge = new GuildWars2Build() { BuildName = "CondiScourge", Class = GuildWars2Class.Necromancer, EliteSpecialization = EliteSpecialization.Scourge };
            GuildWars2Build quickBrand = new GuildWars2Build() { BuildName = "QuickBrand", Class = GuildWars2Class.Guard, EliteSpecialization = EliteSpecialization.Firebrand, Heal = 5, Quickness = 5 };
            GuildWars2Build alacregate = new GuildWars2Build() { BuildName = "Alacregate", Class = GuildWars2Class.Revenant, EliteSpecialization = EliteSpecialization.Renegade, Alacrity = 5 };
            GuildWars2Build chrono = new GuildWars2Build() { BuildName = "Chrono", Class = GuildWars2Class.Mesmer, EliteSpecialization = EliteSpecialization.Chronomancer, Alacrity = 5, Quickness = 5 };
            GuildWars2Build daredevil = new GuildWars2Build() { BuildName = "Daredevil", Class = GuildWars2Class.Thief, EliteSpecialization = EliteSpecialization.DareDevil };
            context.GuildWars2Builds.AddRange(new List<GuildWars2Build>(){healTempest, condiScourge, quickBrand, alacregate, chrono, daredevil });
            context.SaveChanges();

            var equippedBuilds = new Equipped[]
            {
                new Equipped(){GuildWars2Account = linaith, CanTank = true, GuildWars2Build = quickBrand},
                new Equipped(){GuildWars2Account = linaith, CanTank = false, GuildWars2Build = healTempest},
                new Equipped(){GuildWars2Account = linaith, CanTank = false, GuildWars2Build = daredevil},
                new Equipped(){GuildWars2Account = sarah, CanTank = false, GuildWars2Build = healTempest},
                new Equipped(){GuildWars2Account = sarah, CanTank = false, GuildWars2Build = daredevil},
                new Equipped(){GuildWars2Account = hierpiepts, CanTank = false, GuildWars2Build = condiScourge},
                new Equipped(){GuildWars2Account = hierpiepts, CanTank = true, GuildWars2Build = quickBrand},
                new Equipped(){GuildWars2Account = hierpiepts, CanTank = false, GuildWars2Build = healTempest},
                new Equipped(){GuildWars2Account = bloodseeker, CanTank = true, GuildWars2Build = chrono},
                new Equipped(){GuildWars2Account = bloodseeker, CanTank = false, GuildWars2Build = alacregate},
                new Equipped(){GuildWars2Account = bloodseeker, CanTank = false, GuildWars2Build = condiScourge},
            };

            context.Equipped.AddRange(equippedBuilds);
            context.SaveChanges();

            var discordMessage = new DiscordRaidMessage()
            {
                DiscordChannelId = 666954070388637697,
                DiscordGuildId = 666953424734257182,
                DiscordMessageId = 1040355092630614087,
                RaidId = raid.RaidId
            };
            context.DiscordRaidMessages.Add(discordMessage);
            context.SaveChanges();

#endif
        }
    }
}
