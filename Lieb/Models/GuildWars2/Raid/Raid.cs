using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public enum RaidType
    {
        Planned = 0,
        RandomWithBoons = 1,
        RandomClasses = 2,
        RandomEliteSpecialization = 3,
    }

    public class Raid : RaidBase
    {
        public int RaidId { get; private set; }

        [Required]
        public DateTimeOffset StartTimeUTC { get; set; } = DateTime.Now;

        [Required]
        public DateTimeOffset EndTimeUTC { get; set; }

        public DateTimeOffset FreeForAllTimeUTC { get; set; }

        public ICollection<RaidSignUp> SignUps { get; set; } = new HashSet<RaidSignUp>();

        public ICollection<RaidLog> RaidLogs { get; set; } = new HashSet<RaidLog>();

        public Raid() { }

        public Raid(RaidTemplate template) : base(template)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(template.TimeZone);
            StartTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.StartTime, timeZone);
            EndTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.EndTime, timeZone);
            FreeForAllTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.FreeForAllTime, timeZone);
        }

    }
}
