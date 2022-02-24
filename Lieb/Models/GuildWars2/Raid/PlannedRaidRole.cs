namespace Lieb.Models.GuildWars2.Raid
{
    public class PlannedRaidRole
    {
        public int PlannedRaidRoleId { get; set; }
        public string Name { get; set; } = String.Empty;
        public int Spots { get; set; }
        public string Description { get; set; } = String.Empty;
    }
}
