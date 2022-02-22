using Lieb.Models;

namespace Lieb.Data
{
    public class DbInitializer
    {
        public static void Initialize(LiebContext context)
        {
            // Look for any students.
            if (context.LiebUsers.Any())
            {
                return;   // DB has been seeded
            }


            List<LiebRole> roles = new List<LiebRole>();
            foreach (string roleName in Constants.Roles.GetAllRoles())
            {
                LiebRole role = new LiebRole()
                {
                    RoleName = roleName
                };
                roles.Add(role);
            }

            context.LiebRoles.AddRange(roles);
            context.SaveChanges();

            var users = new LiebUser[]
            {
                new LiebUser{DiscordUserId=194863625477816321, Name="Sarah", Birthday=DateTime.Parse("1992-01-15")},
                new LiebUser{DiscordUserId=1, Name="Lisa"},
                new LiebUser{DiscordUserId=2, Name="Simon"}
            };

            context.LiebUsers.AddRange(users);
            context.SaveChanges();


            var assignments = new RoleAssignment[]
            {
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.Admin).LiebRoleId },
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.GuildLead).LiebRoleId },
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.RaidLead).LiebRoleId },
                new RoleAssignment{LiebUserId = users[0].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.User).LiebRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.Admin).LiebRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.GuildLead).LiebRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.RaidLead).LiebRoleId },
                new RoleAssignment{LiebUserId = users[1].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.User).LiebRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.GuildLead).LiebRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.RaidLead).LiebRoleId },
                new RoleAssignment{LiebUserId = users[2].LiebUserId, LiebRoleId = roles.FirstOrDefault(x => x.RoleName == Constants.Roles.User).LiebRoleId }
            };

            context.RoleAssignments.AddRange(assignments);
            context.SaveChanges();

        }
    }
}
