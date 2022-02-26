using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidReminder
    {
        public enum ReminderType
        {
            User = 0,
            Channel = 1
        }

        public RaidReminder(ReminderType type, string message, double hoursBeforeRaid, ulong channelId = 0)
        {
            Type = type;
            Message = message;
            HoursBeforeRaid = hoursBeforeRaid;
            ChannelId = channelId;
        }

        public int RaidReminderId { get; set; }

        public ReminderType Type { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Message too long (1000 character limit).")]
        public string Message { get; set; }

        public double HoursBeforeRaid { get; set; }

        public ulong ChannelId { get; set; }

        public bool Sent { get; set; } = false;
    }
}
