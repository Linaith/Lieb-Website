namespace Lieb.Models
{
    public class Equipped
    {
        public int EquippedId { get; set; }
        public int GuildWars2AccountId { get; set; }
        public int RaidRoleId { get; set; }
        public GuildWars2Account GuildWars2Account { get; set; } = new GuildWars2Account();
        public RaidRole RaidRole { get; set; } = new RaidRole();
    }
}
