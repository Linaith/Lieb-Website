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
        public int UserId { get; set; }
        public int GuildWars2AccountId { get; set; }
        public int PlannedRaidRoleId { get; set; }

        public SignUpType SignUpType { get; set; }

        public Raid Raid { get; set; }
        public LiebUser User { get; set; } = new LiebUser();
        public GuildWars2Account GuildWars2Account { get; set; } = new GuildWars2Account();
        public PlannedRaidRole PlannedRaidRole { get; set; } = new PlannedRaidRole();
    }
}
