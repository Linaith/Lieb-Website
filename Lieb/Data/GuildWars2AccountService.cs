using Lieb.Models;
using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class GuildWars2AccountService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private readonly DiscordService _discordService;

        public GuildWars2AccountService(IDbContextFactory<LiebContext> contextFactory, DiscordService discordService)
        {
            _contextFactory = contextFactory;
            _discordService = discordService;
        }

        public GuildWars2Account GetAccount(int gw2AccountId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.GuildWars2Accounts
                .Include(a => a.EquippedBuilds)
                .ThenInclude(e => e.GuildWars2Build)
                .FirstOrDefault(a => a.GuildWars2AccountId == gw2AccountId);
        }

        public async Task AddOrEditAccount(GuildWars2Account account, ulong userId)
        {
            if (account != null)
            {
                using var context = _contextFactory.CreateDbContext();
                if (account.GuildWars2AccountId == 0)
                {
                    LiebUser? user = context.LiebUsers.Include(u => u.GuildWars2Accounts).FirstOrDefault(u => u.Id == userId);
                    if(user != null)
                    {
                        user.GuildWars2Accounts.Add(account);
                        await context.SaveChangesAsync();
                        if(user.GuildWars2Accounts.Count == 1)
                        {
                            user.MainGW2Account = account.GuildWars2AccountId;
                            await _discordService.RenameUser(userId, user.Name, account.AccountName);
                        }
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    context.Update(account);
                    await context.SaveChangesAsync();
                    LiebUser? user = context.LiebUsers.Include(u => u.GuildWars2Accounts).FirstOrDefault(u => u.Id == userId);
                    if(user != null && user.MainGW2Account == account.GuildWars2AccountId)
                    {
                        await _discordService.RenameUser(userId, user.Name, account.AccountName);
                    }
                }
            }
        }

        public async Task DeleteAccount(int accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            GuildWars2Account? account = await context.GuildWars2Accounts.FirstOrDefaultAsync(b => b.GuildWars2AccountId == accountId);
            if (account != null)
            {
                context.Equipped.RemoveRange(account.EquippedBuilds);
                IEnumerable<RaidSignUp> signUpsToDelete = context.RaidSignUps.Where(s => s.GuildWars2AccountId == accountId);
                HashSet<int> raidsToUpdate = signUpsToDelete.Select(s => s.RaidId).ToHashSet();
                context.RaidSignUps.RemoveRange(signUpsToDelete);
                await context.SaveChangesAsync();
                context.GuildWars2Accounts.Remove(account);
                LiebUser? user = context.LiebUsers.Include(u => u.GuildWars2Accounts).FirstOrDefault(u => u.GuildWars2Accounts.Contains(account));
                await context.SaveChangesAsync();
                if(user != null && user.MainGW2Account == account.GuildWars2AccountId)
                {
                    GuildWars2Account newMain = user.GuildWars2Accounts.FirstOrDefault(new GuildWars2Account());
                    user.MainGW2Account = newMain.GuildWars2AccountId;
                    await context.SaveChangesAsync();
                    await _discordService.RenameUser(user.Id, user.Name, newMain.AccountName);
                }
                foreach(int raidId in raidsToUpdate)
                {
                    await _discordService.PostRaidMessage(raidId);
                }
            }
        }

        public async Task AddBuild(int accountId, int buildId)
        {
            using var context = _contextFactory.CreateDbContext();
            GuildWars2Account? account = context.GuildWars2Accounts
                .Include(a => a.EquippedBuilds)
                .FirstOrDefault(a => a.GuildWars2AccountId == accountId);

            if (account != null)
            {
                account.EquippedBuilds.Add(new Equipped()
                {
                    GuildWars2AccountId = accountId,
                    GuildWars2BuildId = buildId
                });
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveBuild(int accountId, int buildId)
        {
            using var context = _contextFactory.CreateDbContext();
            GuildWars2Account? account = context.GuildWars2Accounts
                .Include(a => a.EquippedBuilds)
                .FirstOrDefault(a => a.GuildWars2AccountId == accountId);
            if (account != null)
            {
                Equipped? buildToRemove = account.EquippedBuilds.FirstOrDefault(b => b.GuildWars2BuildId == buildId);
                if (buildToRemove != null)
                {
                    account.EquippedBuilds.Remove(buildToRemove);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task ChangeTankStatus(int accountId, int buildId, bool canTank)
        {
            using var context = _contextFactory.CreateDbContext();
            GuildWars2Account? account = context.GuildWars2Accounts
                .Include(a => a.EquippedBuilds)
                .FirstOrDefault(a => a.GuildWars2AccountId == accountId);
            if (account != null)
            {
                Equipped? build = account.EquippedBuilds.FirstOrDefault(b => b.GuildWars2BuildId == buildId);
                if (build != null)
                {
                    build.CanTank = canTank;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
