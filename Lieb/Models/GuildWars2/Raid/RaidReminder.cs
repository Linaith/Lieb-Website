using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidReminder
    {
        public enum ReminderType
        {
            User = 1,
            Channel = 2
        }

        public int RaidReminderId { get; set; }

        [Required]
        [Range(1, 2, ErrorMessage = "Please select a reminder type")]
        public ReminderType Type { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Message too long (1000 character limit).")]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset ReminderTime { get; set; } = DateTime.Now;

        public ulong DiscordServerId { get; set; }

        public ulong DiscordChannelId { get; set; }

        public bool Sent { get; set; } = false;
        
        public int RaidId { get; set; }

        public Raid Raid { get; set; }
    }
}
