using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public class Raid : RaidBase
    {
        public int RaidId { get; private set; }

        [Required]
        public DateTimeOffset StartTimeUTC { get; set; } = DateTime.Now;

        [Required]
        public DateTimeOffset EndTimeUTC { get; set; }

        public DateTimeOffset FreeForAllTimeUTC { get; set; }

        public DateTimeOffset MinUserDeadLineUTC { get; set; }

        public int? MinUserPollId { get; set; }

        public ICollection<RaidSignUp> SignUps { get; set; } = new HashSet<RaidSignUp>();

        public Raid() { }

        public Raid(RaidTemplate template) : base(template, template.TimeZone, true)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(template.TimeZone);
            StartTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.StartTime, timeZone);
            EndTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.EndTime, timeZone);
            FreeForAllTimeUTC = TimeZoneInfo.ConvertTimeToUtc(template.FreeForAllTime, timeZone);
            MinUserDeadLineUTC = TimeZoneInfo.ConvertTimeToUtc(template.MinUserDeadLine, timeZone);
        }

    }
}
