using Lieb.Models;
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

        public async Task<LiebUser> GetLiebUser(ulong discordId)
        {
            if (discordId > 0)
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.LiebUsers
                    .Include(u => u.GuildWars2Accounts)
                    .ThenInclude(a => a.EquippedBuilds)
                    .ThenInclude(b => b.GuildWars2Build)
                    .Include(u => u.RoleAssignments)
                    .ThenInclude(r => r.LiebRole)
                    .FirstOrDefaultAsync(u => u.DiscordUserId == discordId);
            }
            else
                return new LiebUser();
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
    }
}
