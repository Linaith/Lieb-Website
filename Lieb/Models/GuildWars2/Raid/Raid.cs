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

    public class Raid
    {
        public int RaidId { get; private set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title too long (100 character limit).")]
        public string Title { get; set; } = String.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Description too long (1000 character limit).")]
        public string Description { get; set; } = String.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public DateTimeOffset StartTime { get; set; }

        [Required]
        public DateTimeOffset EndTime { get; set; }

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

        public bool IsRandomized { get; set; } = false;

        public int Frequency { get; set; }

        public string RequiredRole { get; set; } = String.Empty;

        public DateTime FreeForAllDate { get; set; } = DateTime.Now;

        public DateTimeOffset FreeForAllTime { get; set; }

        //role name, number of spots
        public ICollection<PlannedRaidRole> Roles { get; set; } = new HashSet<PlannedRaidRole>();

        public ICollection<RaidReminder> Reminders { get; set; } = new List<RaidReminder>();

        public ICollection<RaidSignUp> SignUps { get; set; } = new HashSet<RaidSignUp>();

        public ICollection<SignUpHistory> SignUpHistory { get; set; } = new HashSet<SignUpHistory>();

        //used to edit the Discord message
        public ulong DiscordMessageId { get; set; }

        public ulong DiscordChannelId { get; set; }

        public ulong DiscordGuildId { get; set; }
    }
}
