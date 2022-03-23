namespace Lieb.Models.GuildWars2.Raid
{
    public enum SignUpType
    {
        SignedUp = 0,
        Maybe = 1,
        Backup = 2,
        Flex = 3,
        SignedOff = 4
    }

    public class RaidSignUp
    {
        public int RaidSignUpId { get; set; }

        public int RaidId { get; set; }
        public int LiebUserId { get; set; }
        public int GuildWars2AccountId { get; set; }
        public int PlannedRaidRoleId { get; set; }

        public SignUpType SignUpType { get; set; }

        public Raid Raid { get; set; }
        public LiebUser LiebUser { get; set; }
        public GuildWars2Account GuildWars2Account { get; set; }
        public RaidRole PlannedRaidRole { get; set; }
    }
}
