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

        public async Task AddAccount(GuildWars2Account guildWars2Account, ulong discordId)
        {
            using var context = _contextFactory.CreateDbContext();
            LiebUser liebUser = await context.LiebUsers.FirstOrDefaultAsync(u => u.DiscordUserId == discordId);
            if (liebUser != null)
            {
                liebUser.GuildWars2Accounts.Add(guildWars2Account);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAccount(int guildWars2AccountId, string accountName, string apiKey)
        {
            using var context = _contextFactory.CreateDbContext();
            GuildWars2Account account = await context.GuildWars2Accounts.FirstOrDefaultAsync(u => u.GuildWars2AccountId == guildWars2AccountId);
            if (account != null)
            {
                account.ApiKey = apiKey;
                if (!string.IsNullOrEmpty(accountName))
                {
                    account.AccountName = accountName;
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveAccount()
        {
            using var context = _contextFactory.CreateDbContext();

        }

        public async Task AddBuildToAccount()
        {
            using var context = _contextFactory.CreateDbContext();

        }

        public async Task RemoveBuildFromAccount()
        {
            using var context = _contextFactory.CreateDbContext();

        }

        public async Task<List<GuildWars2Build>> GetBuilds()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.GuildWars2Builds.ToList();
        }

        public async Task CreateBuild()
        {
            using var context = _contextFactory.CreateDbContext();
            await context.SaveChangesAsync();
        }

        public async Task UpdateBuild()
        {
            using var context = _contextFactory.CreateDbContext();
            await context.SaveChangesAsync();
        }

        public async Task DeleteBuild()
        {
            using var context = _contextFactory.CreateDbContext();
            await context.SaveChangesAsync();
        }
    }
}
