namespace Lieb.Models.Raid
{
    public class RandomRaid : Raid
    {
        public short Might { get; set; }
        public short Quickness { get; set; }
        public short Alacrity { get; set; }
        public short Heal { get; set; }
        public LiebUser Tank { get; set; } = new LiebUser();

        public bool RandomClass { get; set; }
        public bool RandomEliteSpecialization { get; set; }
    }
}
