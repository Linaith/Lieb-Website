namespace Lieb.Models.Raid
{
    public class RandomRaid : Raid
    {
        public ICollection<Role> WantedRoles { get; set; } = new HashSet<Role>();
    }
}
