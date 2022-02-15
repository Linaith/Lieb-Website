namespace Lieb.Models.Raid
{
    public abstract class Raid
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

        public ICollection<RaidReminder> Reminders { get; set; } = new List<RaidReminder>();

        public ICollection<RaidSignUp> SignUps { get; set; } = new HashSet<RaidSignUp>();

        public ICollection<SignUpHistory> SignUpHistory { get; set; } = new HashSet<SignUpHistory>();

        //used to edit the Discord message
        public ulong DiscordMessageId { get; set; }

        public ulong DiscordChannelId { get; set; }

        public ulong DiscordGuildId { get; set; }
    }
}
