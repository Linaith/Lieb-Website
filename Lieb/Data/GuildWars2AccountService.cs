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

        public async Task AddOrEditAccount(GuildWars2Account account, int userId)
        {
            if (account != null)
            {
                using var context = _contextFactory.CreateDbContext();
                if (account.GuildWars2AccountId == 0)
                {
                    //context.GuildWars2Accounts.Add(account);
                    LiebUser user = context.LiebUsers.FirstOrDefault(u => u.LiebUserId == userId);
                    if(user != null)
                    {
                        user.GuildWars2Accounts.Add(account);
                    }
                    await context.SaveChangesAsync();
                }
                else
                {
                    GuildWars2Account accountToChange = context.GuildWars2Accounts
                        .Include(a => a.EquippedBuilds)
                        .Include(e => e.EquippedBuilds)
                        .FirstOrDefault(a => a.GuildWars2AccountId == account.GuildWars2AccountId);

                    accountToChange.AccountName = account.AccountName;
                    accountToChange.ApiKey = account.ApiKey;

                    List<Equipped> toDelete = new List<Equipped>();
                    foreach (Equipped equipped in accountToChange.EquippedBuilds)
                    {
                        Equipped? newEquipped = account.EquippedBuilds.FirstOrDefault(r => r.EquippedId == equipped.EquippedId);
                        if (newEquipped != null)
                        {
                            equipped.CanTank = newEquipped.CanTank;
                        }
                        else
                        {
                            toDelete.Add(equipped);
                        }
                    }
                    foreach(Equipped equipped in toDelete)
                    {
                        accountToChange.EquippedBuilds.Remove(equipped);
                        context.Equipped.Remove(equipped);
                    }
                    foreach (Equipped equipped in account.EquippedBuilds.Where(r => r.EquippedId == 0))
                    {
                        accountToChange.EquippedBuilds.Add(equipped);
                    }

                    await context.SaveChangesAsync();
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
                await context.SaveChangesAsync();
                context.GuildWars2Accounts.Remove(account);
                await context.SaveChangesAsync();
            }
        }
    }
}
