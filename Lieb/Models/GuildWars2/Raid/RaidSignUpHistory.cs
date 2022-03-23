namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidSignUpHistory
    {
        public int RaidSignUpHistoryId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public DateTimeOffset Time { get; set; } = DateTimeOffset.Now;

        public SignUpType SignUpType { get; set; }

        public Raid Raid { get; set; }
    }
}
