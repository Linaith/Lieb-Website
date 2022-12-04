using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2
{
    public enum Role
    {
        Might = 0,
        Quickness = 1,
        Alacrity = 2,
        Heal = 3,
        Tank = 4,
        pDps = 5,
        cDps = 6,
    }

    public enum GuildWars2Class
    {
        Elementalist = 1,
        Mesmer = 2,
        Necromancer = 3,
        Engineer = 4,
        Ranger = 5,
        Thief = 6,
        Guard = 7,
        Revenant = 8,
        Warrior = 9,
    }

    public enum EliteSpecialization
    {
        Elemantalist = 1,
        Tempest = 2,
        Weaver = 3,
        Catalyst = 4,
        Mesmer = 5,
        Chronomancer = 6,
        Mirage = 7,
        Virtuoso = 8,
        Necromancer = 9,
        Reaper = 10,
        Scourge = 11,
        Harbinger = 12,
        Engineer = 13,
        Scrapper = 14,
        Holosmith = 15,
        Mechanist = 16,
        Ranger = 17,
        Druid = 18,
        Soulbeast = 19,
        Untamed = 20,
        Thief = 21,
        DareDevil = 22,
        Deadeye = 23,
        Spectre = 24,
        Guard = 25,
        Dragonhunter = 26,
        Firebrand = 27,
        Willbender = 28,
        Revenant = 29,
        Herald = 30,
        Renegade = 31,
        Vindicator = 32,
        Warrior = 33,
        Berserker = 34,
        Spellbreaker = 35,
        Bladesworn = 36,
    }

    public class GuildWars2Build
    {
        public int GuildWars2BuildId { get; set; }

        [Required]
        [StringLength(60, ErrorMessage = "BuildName too long (60 character limit).")]
        public string BuildName { get; set; } = String.Empty;

        public short Might { get; set; }
        public short Quickness { get; set; }
        public short Alacrity { get; set; }
        public short Heal { get; set; }

        [Required]
        [Range(1, 9, ErrorMessage = "Please select a class")]
        public GuildWars2Class Class { get; set; }

        [Required]
        [Range(1, 90, ErrorMessage = "Please select an elite specialization")]
        public EliteSpecialization EliteSpecialization { get; set; }

        public ICollection<Equipped> EquippedRoles { get; set; } = new List<Equipped>();

    }
}
