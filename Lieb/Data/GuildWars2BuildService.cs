using Lieb.Models.GuildWars2;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class GuildWars2BuildService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;

        public GuildWars2BuildService(IDbContextFactory<LiebContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddOrEditBuild(GuildWars2Build build)
        {
            if (build != null)
            {
                using var context = _contextFactory.CreateDbContext();
                if (build.GuildWars2BuildId == 0)
                {
                    context.GuildWars2Builds.Add(build);
                }
                else
                {
                    context.Update(build);
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteBuild(int buildId)
        {
            using var context = _contextFactory.CreateDbContext();
            GuildWars2Build? build = await context.GuildWars2Builds.FirstOrDefaultAsync(b => b.GuildWars2BuildId == buildId);
            if (build != null)
            {
                context.GuildWars2Builds.Remove(build);
                await context.SaveChangesAsync();
            }
        }

        public List<GuildWars2Build> GetBuilds()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.GuildWars2Builds.ToList();
        }

        public GuildWars2Build GetBuild(int buildId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.GuildWars2Builds.FirstOrDefault(b => b.GuildWars2BuildId == buildId);
        }
    }
}
