using Lieb.Models;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class DbInitializer
    {
        public static void Initialize(LiebContext context)
        {
            //add new Roles
            List<LiebRole> roles = new List<LiebRole>();
            foreach (string roleName in Constants.Roles.GetAllRoles())
            {
                if (context.LiebRoles.FirstOrDefault(r => r.RoleName == roleName) == null)
                {
                    roles.Add(new LiebRole()
                    {
                        RoleName = roleName
                    });
                }
            }
            context.LiebRoles.AddRange(roles);
            context.SaveChanges();


            // Look for any LiebUsers.
            if (context.LiebUsers.Any())
            {
                return;   // DB has been seeded
            }


            var users = new LiebUser[]
            {
                new LiebUser{DiscordUserId=194863625477816321, Name="Sarah", Birthday=DateTime.Parse("1992-01-15")},
                new LiebUser{DiscordUserId=1, Name="Lisa"},
                new LiebUser{DiscordUserId=2, Name="Simon"}
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
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = UserRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = AdminRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = GuildLeadRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = RaidLeadRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = UserRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = GuildLeadRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = RaidLeadRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = UserRoleId }
            };

            context.RoleAssignments.AddRange(assignments);
            context.SaveChanges();
        }
    }
}
