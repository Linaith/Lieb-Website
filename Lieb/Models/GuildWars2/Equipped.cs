namespace Lieb.Models.GuildWars2
{
    public class Equipped
    {
        public int EquippedId { get; set; }

        public bool CanTank { get; set; }

        public int GuildWars2AccountId { get; set; }
        public int GuildWars2BuildId { get; set; }
        public GuildWars2Account GuildWars2Account { get; set; }
        public GuildWars2Build GuildWars2Build { get; set; }
    }
}
