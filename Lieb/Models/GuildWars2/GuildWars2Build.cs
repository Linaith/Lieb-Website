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
        Engineer = 2,
        Thief = 3,
        Ranger = 4,
        Necromancer = 5,
        Mesmer = 6,
        Revenant = 7,
        Guard = 8,
        Warrior = 9,
    }

    public enum EliteSpecialization
    {
        Elemantalist = 1,
        Tempest = 2,
        Weaver = 3,
        Catalyst = 4,
        Engineer = 5,
        Scrapper = 6,
        Holosmith = 7,
        Mechanist = 8,
        Thief = 9,
        DareDevil = 10,
        Deadeye = 11,
        Spectre = 12,
        Ranger = 13,
        Druid = 14,
        Soulbeast = 15,
        Untamed = 16,
        Necromancer = 17,
        Reaper = 18,
        Scourge = 19,
        Harbinger = 20,
        Mesmer = 21,
        Chronomancer = 22,
        Mirage = 23,
        Virtuoso = 24,
        Revenant = 25,
        Herald = 26,
        Renegade = 27,
        Vindicator = 28,
        Guard = 29,
        Dragonhunter = 30,
        Firebrand = 31,
        Willbender = 32,
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

        public bool Might { get; set; }
        public bool Quickness { get; set; }
        public bool Alacrity { get; set; }
        public bool Heal { get; set; }

        [Required]
        [Range(1,9, ErrorMessage ="Please select a class")]
        public GuildWars2Class Class { get; set; }

        [Required]
        [Range(1, 90, ErrorMessage = "Please select an elite specialization")]
        public EliteSpecialization EliteSpecialization { get; set; }
        public ICollection<Equipped> EquippedRoles { get; set; } = new List<Equipped>();

    }
}
