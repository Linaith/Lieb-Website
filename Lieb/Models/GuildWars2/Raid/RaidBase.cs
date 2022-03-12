using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidBase
    {
        [Required]
        [StringLength(100, ErrorMessage = "Title too long (100 character limit).")]
        public string Title { get; set; } = String.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Description too long (1000 character limit).")]
        public string Description { get; set; } = String.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Organizer too long (50 character limit).")]
        public string Organizer { get; set; } = String.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Guild too long (50 character limit).")]
        public string Guild { get; set; } = String.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "VoiceChat too long (50 character limit).")]
        public string VoiceChat { get; set; } = String.Empty;

        [Required]
        public RaidType RaidType { get; set; }

        public string RequiredRole { get; set; } = String.Empty;

        //role name, number of spots
        public ICollection<PlannedRaidRole> Roles { get; set; } = new HashSet<PlannedRaidRole>();

        public ICollection<RaidReminder> Reminders { get; set; } = new List<RaidReminder>();

        //used to edit the Discord message
        public ulong DiscordChannelId { get; set; }

        public ulong DiscordGuildId { get; set; }
    }
}
