using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public enum RaidType
    {
        Planned = 0,
        RandomWithBoons = 1,
        RandomClasses = 2,
        RandomEliteSpecialization = 3,
    }

    public class Raid : RaidBase
    {
        public int RaidId { get; private set; }

        [Required]
        public DateTimeOffset StartTimeUTC { get; set; } = DateTime.Now;

        [Required]
        public DateTimeOffset EndTimeUTC { get; set; }

        public DateTimeOffset FreeForAllTimeUTC { get; set; }

        public bool IsRandomized { get; set; } = false;

        public ICollection<RaidSignUp> SignUps { get; set; } = new HashSet<RaidSignUp>();

        public ICollection<SignUpHistory> SignUpHistory { get; set; } = new HashSet<SignUpHistory>();

        //used to edit the Discord message
        public ulong DiscordMessageId { get; set; }

        public Raid() { }

        public Raid(RaidTemplate template)
        {
            this.Title = template.Title;
            this.Description = template.Description;
            this.Organizer = template.Organizer;
            this.Guild = template.Guild;
            this.VoiceChat = template.VoiceChat;
            this.RaidType = template.RaidType;
            this.RequiredRole = template.RequiredRole;
            this.DiscordChannelId = template.DiscordChannelId;
            this.DiscordGuildId = template.DiscordGuildId;

            foreach(PlannedRaidRole role in template.Roles)
            {
                this.Roles.Add(new PlannedRaidRole()
                {
                    Description = role.Description,
                    Name = role.Name,
                    Spots = role.Spots
                });
            }
            foreach(RaidReminder reminder in template.Reminders)
            {
                this.Reminders.Add(new RaidReminder()
                {
                    ChannelId = reminder.ChannelId,
                    HoursBeforeRaid = reminder.HoursBeforeRaid,
                    Message = reminder.Message,
                    Sent = reminder.Sent,
                    Type = reminder.Type
                });
            }

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(template.TimeZone);
            StartTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.StartTime, timeZone);
            EndTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.EndTime, timeZone);
            FreeForAllTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.FreeForAllTime, timeZone);
        }

    }
}
