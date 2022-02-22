namespace Lieb.Models.GuildWars2.Raid
{
    public enum RaidType
    {
        Planned = 1,
        RandomWithBoons = 2,
        RandomClasses = 3,
        RandomEliteSpecialization = 4,
    }

    public class Raid
    {
        public int RaidId { get; private set; }

        public string Title { get; set; } = String.Empty;

        public string Description { get; set; } = String.Empty;

        public DateTimeOffset StartTime { get; set; }

        public double RaidDuration { get; set; }

        public string Organisator { get; set; } = String.Empty;

        public string Guild { get; set; } = String.Empty;

        public string VoiceChat { get; set; } = String.Empty;

        public int Frequency { get; set; }

        public RaidType RaidType { get; set; }

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
