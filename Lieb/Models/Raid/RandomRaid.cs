namespace Lieb.Models.Raid
{
    public class RandomRaid : Raid
    {
        public short Might { get; set; }
        public short Quickness { get; set; }
        public short Alacrity { get; set; }
        public short Heal { get; set; }
        public User Tank { get; set; } = new User();

        public bool RandomClass { get; set; }
        public bool RandomEliteSpecialization { get; set; }
    }
}
