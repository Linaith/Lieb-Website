using Lieb.Models;
using Lieb.Models.GuildWars2;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class GuildWars2AccountService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;

        public GuildWars2AccountService(IDbContextFactory<LiebContext> contextFactory)
        {
            _contextFactory = contextFactory;
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
                    LiebUser? user = context.LiebUsers.FirstOrDefault(u => u.Id == userId);
                    if(user != null)
                    {
                        user.GuildWars2Accounts.Add(account);
                    }
                }
                else
                {
                    context.Update(account);
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAccount(int accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            GuildWars2Account? account = await context.GuildWars2Accounts.FirstOrDefaultAsync(b => b.GuildWars2AccountId == accountId);
            if (account != null)
            {
                context.Equipped.RemoveRange(account.EquippedBuilds);
                await context.SaveChangesAsync();
                context.GuildWars2Accounts.Remove(account);
                await context.SaveChangesAsync();
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
