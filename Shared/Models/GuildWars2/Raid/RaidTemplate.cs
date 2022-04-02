using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidTemplate : RaidBase
    {
        public int RaidTemplateId { get; private set; }

        [Required]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Required]
        public DateTime EndTime { get; set; }

        public DateTime FreeForAllTime { get; set; }

        public string TimeZone { get; set; } = String.Empty;

        public int Interval { get; set; }
        
        public int CreateDaysBefore { get; set; }
    }
}
