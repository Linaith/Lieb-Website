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
                    .FirstOrDefault(u => u.Id == userId, new LiebUser());

                if(user == null) return new GuildWars2Account();

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

        public async Task DeleteUser(ulong userId)
        {
            using var context = _contextFactory.CreateDbContext();
            IEnumerable<Raid> raids = context.Raids.Where(r => r.RaidOwnerId == userId);
            foreach(Raid raid in raids)
            {
                raid.RaidOwnerId = null;
            }
            IEnumerable<RaidTemplate> templates = context.RaidTemplates.Where(r => r.RaidOwnerId == userId);
            foreach(RaidTemplate template in templates)
            {
                template.RaidOwnerId = null;
            }
            await context.SaveChangesAsync();

            IEnumerable<RaidSignUp> signUps = context.RaidSignUps.Where(r => r.LiebUserId == userId);
            HashSet<int> raidIds = signUps.Select(s => s.RaidId).ToHashSet();
            context.RemoveRange(signUps);
            await context.SaveChangesAsync();
            foreach(int raidId in raidIds)
            {
                await _discordService.PostRaidMessage(raidId);
            }

            IEnumerable<RaidLog> logs = context.RaidLogs.Where(r => r.UserId == userId);
            foreach(RaidLog log in logs)
            {
                log.UserId = null;
            }
            await context.SaveChangesAsync();

            LiebUser user = GetLiebUser(userId);
            foreach(GuildWars2Account account in user.GuildWars2Accounts)
            {
                await _guildWars2AccountService.DeleteAccount(account.GuildWars2AccountId);
            }
            user.GuildWars2Accounts.Clear();

            if(user.BannedUntil > DateTime.Now)
            {
                LiebUser contextUser = context.LiebUsers.First(u => u.Id == userId);
                contextUser.Name = "Deleted and Banned";
                contextUser.MainGW2Account = 0;
                contextUser.Pronouns = string.Empty;
                contextUser.Birthday = null;
                contextUser.AlwaysSignUpWithMainAccount = false;
                LiebRole standardRole = await context.LiebRoles.FirstOrDefaultAsync(m => m.RoleName == Constants.Roles.User.Name);
                context.RemoveRange(user.RoleAssignments.Where(a => a.LiebRoleId != standardRole.LiebRoleId));
            }
            else
            {
                context.Remove(user);
            }
            await context.SaveChangesAsync();
        }

        public async Task UpdateBannedUntil(ulong userId, DateTime? date)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser? user = await context.LiebUsers
                .Include(u => u.RoleAssignments)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return;

            user.BannedUntil = date;
            if(user.BannedUntil > DateTime.UtcNow)
            {
                LiebRole standardRole = await context.LiebRoles.FirstOrDefaultAsync(m => m.RoleName == Constants.Roles.User.Name);
                context.RemoveRange(user.RoleAssignments.Where(a => a.LiebRoleId != standardRole.LiebRoleId));
            }
            await context.SaveChangesAsync();

            //sign off from every Raid
            IEnumerable<RaidSignUp> signUps = context.RaidSignUps.Where(r => r.LiebUserId == userId);
            HashSet<int> raidIds = signUps.Select(s => s.RaidId).ToHashSet();
            context.RemoveRange(signUps);
            await context.SaveChangesAsync();
            foreach(int raidId in raidIds)
            {
                await _discordService.PostRaidMessage(raidId);
            }
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
                    .FirstOrDefault(r => r.RaidId == raidId);
                
            if(raid == null) return new List<GuildWars2Account>();
                    
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
                    .FirstOrDefault(r => r.RaidId == raidId);

            if(raid == null) return 0;
                    
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
