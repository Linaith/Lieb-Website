using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidReminder
    {
        public enum ReminderType
        {
            User = 1,
            Channel = 2,
            Group = 3
        }

        public enum ReminderTimeType
        {
            Static = 1,
            Dynamic = 2
        }

        public enum RoleReminderType
        {
            All = 0,
            SignedUp = 1,
            NotSignedUp = 2
        }

        public int RaidReminderId { get; set; }

        [Required]
        [Range(1, 3, ErrorMessage = "Please select a reminder type")]
        public ReminderType Type { get; set; }

        [Required]
        [Range(1, 2, ErrorMessage = "Please select a reminder type")]
        public ReminderTimeType TimeType { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Message too long (1000 character limit).")]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset ReminderTimeUTC { get; set; } = DateTime.Now;

        public ulong DiscordServerId { get; set; }

        public ulong DiscordChannelId { get; set; }

        public int RoleId {get; set; }
        
        public RoleReminderType RoleType {get; set;}

        public bool Sent { get; set; } = false;
    }

    public class StaticRaidReminder : RaidReminder
    {
        public DateTimeOffset ReminderDate {get; set; } = DateTime.Now.Date;
        public DateTimeOffset ReminderTime {get; set; }

        
        public StaticRaidReminder()
        {
            TimeType = ReminderTimeType.Static;
        }

        public StaticRaidReminder(RaidReminder reminder, DateTimeOffset reminderDate, DateTimeOffset remindertime)
        {
            var properties = reminder.GetType().GetProperties();
            properties.ToList().ForEach(property =>
            {
                var value = reminder.GetType().GetProperty(property.Name).GetValue(reminder, null);
                this.GetType().GetProperty(property.Name).SetValue(this, value, null);
            });
            ReminderDate = reminderDate;
            ReminderTime = remindertime;
        }
    }

    public class DynamicRaidReminder : RaidReminder
    {
        public int DaysBeforeRaid {get; set; }
        public int HoursBeforeRaid {get; set; }
        public int MinutesBeforeRaid {get; set; }

        public DynamicRaidReminder()
        {
            TimeType = ReminderTimeType.Dynamic;
        }

        public DynamicRaidReminder(RaidReminder reminder, DateTimeOffset raidStartTimeUTC)
        {
            var properties = reminder.GetType().GetProperties();
            properties.ToList().ForEach(property =>
            {
                var value = reminder.GetType().GetProperty(property.Name).GetValue(reminder, null);
                this.GetType().GetProperty(property.Name).SetValue(this, value, null);
            });
            TimeSpan reminderOffset = raidStartTimeUTC - reminder.ReminderTimeUTC;
            DaysBeforeRaid = (int)reminderOffset.TotalDays;
            HoursBeforeRaid = (int)(reminderOffset.TotalHours % 24);
            MinutesBeforeRaid = (int)(reminderOffset.TotalMinutes % 60);
        }

        public static DynamicRaidReminder Create30MinReminder()
        {
            return new DynamicRaidReminder(){
                DaysBeforeRaid = 0,
                HoursBeforeRaid = 0,
                MinutesBeforeRaid = 30,
                Message = "The raid starts in 30 minutes.",
                TimeType = ReminderTimeType.Dynamic,
                Type = ReminderType.User                
            };
        }
    }
}
