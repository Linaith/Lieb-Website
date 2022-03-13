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

        public RaidTemplate GetTemplate(int raidTemplateId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.RaidTemplates
                .Include(r => r.Roles)
                .Include(r => r.Reminders)
                .FirstOrDefault(t => t.RaidTemplateId == raidTemplateId);
        }

        public async Task AddOrEditTemplate(RaidTemplate template)
        {
            if (template != null)
            {
                using var context = _contextFactory.CreateDbContext();
                if (template.RaidTemplateId == 0)
                {
                    context.RaidTemplates.Add(template);
                    await context.SaveChangesAsync();
                }
                else
                {
                    RaidTemplate raidToChange = await context.RaidTemplates
                        .Include(r => r.Roles)
                        .Include(r => r.Reminders)
                        .FirstOrDefaultAsync(r => r.RaidTemplateId == template.RaidTemplateId);
                    raidToChange.Title = template.Title;
                    raidToChange.Description = template.Description;
                    raidToChange.Organizer = template.Organizer;
                    raidToChange.Guild = template.Guild;
                    raidToChange.VoiceChat = template.VoiceChat;
                    raidToChange.RaidType = template.RaidType;
                    raidToChange.RequiredRole = template.RequiredRole;
                    raidToChange.DiscordChannelId = template.DiscordChannelId;
                    raidToChange.DiscordGuildId = template.DiscordGuildId;
                    raidToChange.StartTime = template.StartTime;
                    raidToChange.EndTime = template.EndTime;
                    raidToChange.FreeForAllTime = template.FreeForAllTime;
                    raidToChange.TimeZone = template.TimeZone;
                    raidToChange.Frequency = template.Frequency;
                    raidToChange.CreateDaysBefore = template.CreateDaysBefore;

                    context.PlannedRaidRoles.RemoveRange(raidToChange.Roles);
                    context.RaidReminders.RemoveRange(raidToChange.Reminders);
                    raidToChange.Roles.Clear();
                    raidToChange.Reminders.Clear();
                    raidToChange.Roles = template.Roles;
                    raidToChange.Reminders = template.Reminders;

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteTemplate(int raidTemplateId)
        {
            using var context = _contextFactory.CreateDbContext();
            RaidTemplate template = GetTemplate(raidTemplateId);
            context.PlannedRaidRoles.RemoveRange(template.Roles);
            context.RaidReminders.RemoveRange(template.Reminders);
            await context.SaveChangesAsync();
            context.RaidTemplates.Remove(template);
            await context.SaveChangesAsync();
        }

        public async Task CreateRaidFromTemplate(int raidTempalteId)
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
