using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class RaidTemplateService
    {

        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private readonly RaidService _raidSerive;

        public RaidTemplateService(IDbContextFactory<LiebContext> contextFactory, RaidService raidSerice)
        {
            _contextFactory = contextFactory;
            _raidSerive = raidSerice;
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
                .Include(r => r.DiscordRaidMessages)
                .Include(r => r.TemplateLogs)
                .FirstOrDefault(t => t.RaidTemplateId == raidTemplateId);
        }

        public async Task AddOrEditTemplate(RaidTemplate template, List<RaidRole> rolesToDelete, List<RaidReminder> remindersToDelete, List<DiscordRaidMessage> messagesToDelete, ulong changedBy)
        {
            if (template != null)
            {
                using var context = _contextFactory.CreateDbContext();
                if (template.RaidTemplateId == 0)
                {
                    context.RaidTemplates.Add(template);
                }
                else
                {
                    context.Update(template);
                    context.RaidRoles.RemoveRange(rolesToDelete);
                    context.RaidReminders.RemoveRange(remindersToDelete);
                    context.DiscordRaidMessages.RemoveRange(messagesToDelete);
                }
                await context.SaveChangesAsync();
                RaidLog log = RaidLog.CreateRaidTemplateLog(changedBy, template);
                await context.RaidLogs.AddAsync(log);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteTemplate(int raidTemplateId)
        {
            using var context = _contextFactory.CreateDbContext();
            RaidTemplate template = GetTemplate(raidTemplateId);
            context.RaidRoles.RemoveRange(template.Roles);
            context.RaidReminders.RemoveRange(template.Reminders);
            context.DiscordRaidMessages.RemoveRange(template.DiscordRaidMessages);
            context.RaidLogs.RemoveRange(template.TemplateLogs);
            await context.SaveChangesAsync();
            template.Roles.Clear();
            template.Reminders.Clear();
            template.DiscordRaidMessages.Clear();
            template.TemplateLogs.Clear();
            context.RaidTemplates.Remove(template);
            await context.SaveChangesAsync();
        }

        public async Task CreateRaidFromTemplate(int raidTempalteId)
        {
            using var context = _contextFactory.CreateDbContext();
            RaidTemplate? template = await context.RaidTemplates
                .Include(r => r.Roles)
                .Include(r => r.Reminders)
                .Include(r => r.DiscordRaidMessages)
                .FirstOrDefaultAsync(t => t.RaidTemplateId == raidTempalteId);
            if(template == null)
            {
                return;
            }
            Raid raid = new Raid(template);
            await _raidSerive.AddOrEditRaid(raid, null, null, null, template.RaidOwnerId);
            MoveTemplate(template);
            await context.SaveChangesAsync();
        }

        private void MoveTemplate(RaidTemplate template)
        {
            template.StartTime = template.StartTime.AddDays(template.Interval);
            template.EndTime = template.EndTime.AddDays(template.Interval);
            template.FreeForAllTime = template.FreeForAllTime.AddDays(template.Interval);
            foreach(RaidReminder reminder in template.Reminders)
            {
                reminder.ReminderTimeUTC = reminder.ReminderTimeUTC.AddDays(template.Interval);
            }
        }
    }
}
