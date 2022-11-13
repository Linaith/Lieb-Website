namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidSignUpHistory
    {
        public int RaidSignUpHistoryId { get; set; }

        public int RaidId { get; set; }

        public ulong UserId {get; set;}
        public int GuildWars2AccountId { get; set; }

        //public ulong SignedUpByUserId {get; set;}

        public string UserName {get; set;} = string.Empty;

        public DateTimeOffset Time { get; set; } = DateTimeOffset.Now;

        public SignUpType SignUpType { get; set; }

        public LiebUser User {get; set;}

        //public LiebUser SignedUpByUser {get; set;}

        public Raid Raid { get; set; }

        public GuildWars2Account GuildWars2Account { get; set; }
    }
}
