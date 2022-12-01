using Lieb.Models;
using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class UserService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private readonly DiscordService _discordService;
        private readonly GuildWars2AccountService _guildWars2AccountService;

        public UserService(IDbContextFactory<LiebContext> contextFactory, DiscordService discordService, GuildWars2AccountService guildWars2AccountService)
        {
            _contextFactory = contextFactory;
            _discordService = discordService;
            _guildWars2AccountService = guildWars2AccountService;
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

        public GuildWars2Account GetMainAccount(ulong userId)
        {
                using var context = _contextFactory.CreateDbContext();
                LiebUser user = context.LiebUsers
                    .Include(u => u.GuildWars2Accounts)
                    .ToList()
                    .FirstOrDefault(u => u.Id == userId, new LiebUser());
                return user.GuildWars2Accounts.FirstOrDefault(g => g.GuildWars2AccountId == user.MainGW2Account, new GuildWars2Account());
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
            context.Update(user);
            await context.SaveChangesAsync();
            await _discordService.RenameUser(user.Id, user.Name, GetMainAccount(user.Id).AccountName);
        }

        public async Task DeleteUser(LiebUser user)
        {
            using var context = _contextFactory.CreateDbContext();
            
            foreach(GuildWars2Account account in user.GuildWars2Accounts)
            {
                await _guildWars2AccountService.DeleteAccount(account.GuildWars2AccountId);
            }
            context.RaidLogs.RemoveRange(context.RaidLogs.Where(r => r.UserId == user.Id).ToList());
            context.RaidSignUps.RemoveRange(context.RaidSignUps.Where(r => r.LiebUserId == user.Id));
            context.RoleAssignments.RemoveRange(context.RoleAssignments.Where(r => r.LiebUserId == user.Id));
            context.Remove(user);
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

        public List<GuildWars2Account> GetAllUsableAccounts(ulong userId, RaidType raidType)
        {
            LiebUser user = GetLiebUserGW2AccountOnly(userId);
            return GetAllUsableAccounts(user, raidType);
        }

        public List<GuildWars2Account> GetAllUsableAccounts(LiebUser user, RaidType raidType)
        {
            if (raidType == RaidType.Planned)
            {
                return user.GuildWars2Accounts.ToList();
            }
            else
            {
                return user.GuildWars2Accounts.Where(a => a.EquippedBuilds.Count > 0).ToList();
            }
        }

        public List<GuildWars2Account> GetDiscordSignUpAccounts(ulong userId, int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser user = GetLiebUserGW2AccountOnly(userId);
            Raid raid = context.Raids
                    .ToList()
                    .FirstOrDefault(r => r.RaidId == raidId, new Raid());
                    
            List<GuildWars2Account> accounts = GetAllUsableAccounts(user, raid.RaidType);
            if(user.AlwaysSignUpWithMainAccount && accounts.Where(a => a.GuildWars2AccountId == user.MainGW2Account).Any())
            {
                return accounts.Where(a => a.GuildWars2AccountId == user.MainGW2Account).ToList();
            }
            else
            {
                return accounts;
            }
        }

        public int GetSignUpAccount(ulong userId, int raidId, int plannedAccountId)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser user = GetLiebUserGW2AccountOnly(userId);
            Raid raid = context.Raids
                    .ToList()
                    .FirstOrDefault(r => r.RaidId == raidId, new Raid());
                    
            List<GuildWars2Account> usableAccounts = GetAllUsableAccounts(user, raid.RaidType);

            if(usableAccounts.Where(a => a.GuildWars2AccountId == plannedAccountId).Any())
            {
                return plannedAccountId;
            }
            if(usableAccounts.Where(a => a.GuildWars2AccountId == user.MainGW2Account).Any())
            {
                return user.MainGW2Account;
            }
            else
            {
                return usableAccounts.First().GuildWars2AccountId;
            }
        }
    }
}
