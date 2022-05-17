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

        public LiebUser GetLiebUser(ulong userId)
        {
            if (userId > 0)
            {
                using var context = _contextFactory.CreateDbContext();
                return context.LiebUsers
                    .Include(u => u.GuildWars2Accounts)
                    .ThenInclude(a => a.EquippedBuilds)
                    .ThenInclude(b => b.GuildWars2Build)
                    .Include(u => u.RoleAssignments)
                    .ThenInclude(r => r.LiebRole)
                    .FirstOrDefault(u => u.Id == userId);
            }
            else
                return new LiebUser();
        }

        public LiebUser GetLiebUserGW2AccountOnly(ulong userId)
        {
            if (userId > 0)
            {
                using var context = _contextFactory.CreateDbContext();
                return context.LiebUsers
                .Include(u => u.GuildWars2Accounts)
                .ThenInclude(a => a.EquippedBuilds)
                .FirstOrDefault(u => u.Id == userId);
            }
            else
                return new LiebUser();
        }

        public async Task CreateUser(ulong discordId, string userName)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser newUser = new LiebUser()
            {
                Id = discordId,
                Name = userName
            };
            context.LiebUsers.Add(newUser);
            await context.SaveChangesAsync();

            LiebRole standardRole = await context.LiebRoles.FirstOrDefaultAsync(m => m.RoleName == Constants.Roles.User.Name);
            if (standardRole != null)
            {
                RoleAssignment roleAssignment = new RoleAssignment()
                {
                    LiebRoleId = standardRole.LiebRoleId,
                    LiebUserId = newUser.Id
                };
                context.RoleAssignments.Add(roleAssignment);
                await context.SaveChangesAsync();
            }
        }

        public async Task EditUser(LiebUser user)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser? userToChange = context.LiebUsers
            .Include(u => u.GuildWars2Accounts)
            .FirstOrDefault(u => u.Id == user.Id);

            if(userToChange != null)
            {
                userToChange.Name = user.Name;
                userToChange.Pronouns = user.Pronouns;
                userToChange.Birthday = user.Birthday;
            }
            await context.SaveChangesAsync();
        }

        public async Task UpdateBannedUntil(ulong userId, DateTime? date)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser? user = await context.LiebUsers.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return;

            user.BannedUntil = date;

            await context.SaveChangesAsync();
        }

        public async Task AddRoleToUser(ulong userId, int roleId)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser? user = await context.LiebUsers
                    .Include(u => u.RoleAssignments)
                    .FirstOrDefaultAsync(u => u.Id == userId);
            user.RoleAssignments.Add(new RoleAssignment()
            {
                LiebUserId = userId,
                LiebRoleId = roleId
            });
            await context.SaveChangesAsync();
        }

        public async Task RemoveRoleFromUser(ulong userId, int roleId)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser? user = await context.LiebUsers
                    .Include(u => u.RoleAssignments)
                    .FirstOrDefaultAsync(u => u.Id == userId);
            RoleAssignment assignmentToRemove = user.RoleAssignments.FirstOrDefault(r => r.LiebRoleId == roleId);
            if(assignmentToRemove != null)
            {
                user.RoleAssignments.Remove(assignmentToRemove);
            }
            await context.SaveChangesAsync();
        }

        public int GetPowerLevel(ulong userId)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser? user = context.LiebUsers
                .Include(u => u.RoleAssignments)
                .ThenInclude(r => r.LiebRole)
                .AsNoTracking()
                .FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                return user.RoleAssignments.Max(a => a.LiebRole.Level);
            }
            return 0;
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
