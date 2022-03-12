using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class RaidTemplateService
    {

        private readonly IDbContextFactory<LiebContext> _contextFactory;

        public RaidTemplateService(IDbContextFactory<LiebContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<RaidTemplate> GetTemplates()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.RaidTemplates
                .Include(r => r.Roles)
                .Include(r => r.Reminders)
                .ToList();
        }

        public async Task CreateNewRaid(int raidTempalteId)
        {
            using var context = _contextFactory.CreateDbContext();
            RaidTemplate? template = await context.RaidTemplates
                .Include(r => r.Roles)
                .Include(r => r.Reminders)
                .FirstOrDefaultAsync(t => t.RaidTemplateId == raidTempalteId);
            if(template == null)
            {
                return;
            }
            Raid raid = new Raid(template);
            context.Raids.Add(raid);
            MoveTemplate(template);
            await context.SaveChangesAsync();
        }

        private void MoveTemplate(RaidTemplate template)
        {
            template.StartTime = template.StartTime.AddDays(template.Frequency);
            template.EndTime = template.EndTime.AddDays(template.Frequency);
            template.FreeForAllTime = template.FreeForAllTime.AddDays(template.Frequency);
        }
    }
}
