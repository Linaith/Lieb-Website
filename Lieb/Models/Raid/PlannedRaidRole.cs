namespace Lieb.Models.Raid
{
    public class PlannedRaidRole
    {
        public int PlannedRaidRoleId { get; set; }
        public string Name { get; set; } = String.Empty;
        public int Spots { get; }
        public string Description { get; set; } = String.Empty;
    }
}
