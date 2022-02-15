namespace Lieb.Models.Raid
{
    public class PlannedRaid : Raid
    {
        //role name, number of spots
        public ICollection<PlannedRaidRole> Roles { get; set; } = new HashSet<PlannedRaidRole>();
    }
}
