using System.Text.Json.Serialization;

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
        public bool IsExternalUser {get { return LiebUserId == null;}}
        public int RaidId { get; set; }
        public ulong? LiebUserId { get; set; }
        public int? GuildWars2AccountId { get; set; }
        public int RaidRoleId { get; set; }
        public string ExternalUserName {get; set;} = string.Empty;

        public SignUpType SignUpType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Raid Raid { get; set; }
        public LiebUser? LiebUser { get; set; }
        public GuildWars2Account? GuildWars2Account { get; set; }
        public RaidRole RaidRole { get; set; }

        public RaidSignUp(int raidId, ulong userId, int gw2AccountId, int roleId, SignUpType signUpType)
        {
            RaidId = raidId;
            LiebUserId = userId;
            GuildWars2AccountId = gw2AccountId;
            RaidRoleId = roleId;
            SignUpType = signUpType;
        }

        public RaidSignUp(int raidId, string userName, int roleId, SignUpType signUpType)
        {
            RaidId = raidId;
            RaidRoleId = roleId;
            SignUpType = signUpType;
            ExternalUserName = userName;
        }
        private RaidSignUp()
        {

        }
    }
}
