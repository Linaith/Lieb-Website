using Lieb.Models;
using Lieb.Models.GuildWars2;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class UserService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;

        public UserService(IDbContextFactory<LiebContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<LiebUser> GetLiebUsers()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.LiebUsers
                .Include(u => u.GuildWars2Accounts)
                .ThenInclude(a => a.EquippedBuilds)
                .ThenInclude(b => b.GuildWars2Build)
                .Include(u => u.RoleAssignments)
                .ThenInclude(r => r.LiebRole)
                .ToList();
        }

        public LiebUser GetLiebUser(ulong discordId)
        {
            if (discordId > 0)
            {
                using var context = _contextFactory.CreateDbContext();
                return context.LiebUsers
                    .Include(u => u.GuildWars2Accounts)
                    .ThenInclude(a => a.EquippedBuilds)
                    .ThenInclude(b => b.GuildWars2Build)
                    .Include(u => u.RoleAssignments)
                    .ThenInclude(r => r.LiebRole)
                    .FirstOrDefault(u => u.DiscordUserId == discordId);
            }
            else
                return new LiebUser();
        }

        public LiebUser GetLiebUser(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.LiebUsers
                .Include(u => u.GuildWars2Accounts)
                .ThenInclude(a => a.EquippedBuilds)
                .ThenInclude(b => b.GuildWars2Build)
                .Include(u => u.RoleAssignments)
                .ThenInclude(r => r.LiebRole)
                .AsNoTracking()
                .FirstOrDefault(u => u.LiebUserId == userId);
        }

        public LiebUser GetLiebUserSmall(ulong discordId)
        {
            if (discordId > 0)
            {
                using var context = _contextFactory.CreateDbContext();
                return context.LiebUsers
                .Include(u => u.GuildWars2Accounts)
                .FirstOrDefault(u => u.DiscordUserId == discordId);
            }
            else
                return new LiebUser();
        }

        public LiebUser GetLiebUserSmall(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.LiebUsers
            .Include(u => u.GuildWars2Accounts)
            .FirstOrDefault(u => u.LiebUserId == userId);
        }

        public async Task<int> GetLiebUserId(ulong discordId)
        {
            if (discordId > 0)
            {
                using var context = _contextFactory.CreateDbContext();
                return (await context.LiebUsers.FirstOrDefaultAsync(u => u.DiscordUserId == discordId)).LiebUserId;
            }
            else
                return -1;
        }

        public async Task EditUser(LiebUser user)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser? userToChange = context.LiebUsers
            .Include(u => u.GuildWars2Accounts)
            .FirstOrDefault(u => u.LiebUserId == user.LiebUserId);

            if(userToChange != null)
            {
                userToChange.Name = user.Name;
                userToChange.Pronouns = user.Pronouns;
                userToChange.Birthday = user.Birthday;
            }
            await context.SaveChangesAsync();
        }

        public async Task EditUserRoles(LiebUser user)
        {
            if (user != null)
            {
                using var context = _contextFactory.CreateDbContext();
                LiebUser? userToChange = await context.LiebUsers
                    .Include(u => u.RoleAssignments)
                    .FirstOrDefaultAsync(u => u.LiebUserId == user.LiebUserId);

                if (userToChange == null)
                    return;

                userToChange.BannedUntil = user.BannedUntil;

                List<RoleAssignment> toDelete = new List<RoleAssignment>();
                foreach (RoleAssignment assignment in userToChange.RoleAssignments)
                {
                    RoleAssignment? newAssignment = user.RoleAssignments.FirstOrDefault(r => r.RoleAssignmentId == assignment.RoleAssignmentId);
                    if (newAssignment == null)
                    {
                        toDelete.Add(assignment);
                    }
                }
                foreach (RoleAssignment assignment in toDelete)
                {
                    userToChange.RoleAssignments.Remove(assignment);
                    context.RoleAssignments.Remove(assignment);
                }
                foreach (RoleAssignment assignment in user.RoleAssignments.Where(r => r.RoleAssignmentId == 0))
                {
                    userToChange.RoleAssignments.Add(assignment);
                }

                await context.SaveChangesAsync();
            }
        }


        public List<LiebRole> GetLiebRoles()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.LiebRoles
                .Include(u => u.RoleAssignments)
                .ThenInclude(r => r.LiebUser)
                .ToList();
        }

        public async Task AddRole(LiebRole role)
        {
            using var context = _contextFactory.CreateDbContext();
            if (context.LiebRoles.FirstOrDefault(r => r.RoleName == role.RoleName) == null)
            {
                context.LiebRoles.Add(role);
            }
            await context.SaveChangesAsync();
        }

        public async Task DeleteRole(int roleId)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebRole role = context.LiebRoles.FirstOrDefault(r => r.LiebRoleId == roleId);
            if (role != null)
            {
                context.LiebRoles.Remove(role);
                await context.SaveChangesAsync();
            }
        }
    }
}
