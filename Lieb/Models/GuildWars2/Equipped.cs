namespace Lieb.Models.GuildWars2
{
    public class Equipped
    {
        public int EquippedId { get; set; }

        public bool CanTank { get; set; }

        public int GuildWars2AccountId { get; set; }
        public int RaidRoleId { get; set; }
        public GuildWars2Account GuildWars2Account { get; set; } = new GuildWars2Account();
        public GuildWars2Build RaidRole { get; set; } = new GuildWars2Build();
    }
}
