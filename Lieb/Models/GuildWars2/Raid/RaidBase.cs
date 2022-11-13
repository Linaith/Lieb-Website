using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public abstract class RaidBase
    {
        [Required]
        [StringLength(100, ErrorMessage = "Title too long (100 character limit).")]
        public string Title { get; set; } = String.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Description too long (1000 character limit).")]
        public string Description { get; set; } = String.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Organizer too long (50 character limit).")]
        public string Organizer { get; set; } = String.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Guild too long (50 character limit).")]
        public string Guild { get; set; } = String.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "VoiceChat too long (50 character limit).")]
        public string VoiceChat { get; set; } = String.Empty;

        [Required]
        public RaidType RaidType { get; set; }

        public string RequiredRole { get; set; } = String.Empty;

        public bool MoveFlexUsers { get; set; } = true;

        public ulong RaidOwnerId { get; set; }

        //role name, number of spots
        public ICollection<RaidRole> Roles { get; set; } = new HashSet<RaidRole>();

        public ICollection<RaidReminder> Reminders { get; set; } = new List<RaidReminder>();

        public ICollection<DiscordRaidMessage> DiscordRaidMessages { get; set; } = new HashSet<DiscordRaidMessage>();

        public RaidBase() { }

        public RaidBase(RaidBase template)
        {
            this.Title = template.Title;
            this.Description = template.Description;
            this.Organizer = template.Organizer;
            this.Guild = template.Guild;
            this.VoiceChat = template.VoiceChat;
            this.RaidType = template.RaidType;
            this.RequiredRole = template.RequiredRole;
            this.MoveFlexUsers = template.MoveFlexUsers;
            this.RaidOwnerId = template.RaidOwnerId;

            foreach (RaidRole role in template.Roles)
            {
                this.Roles.Add(new RaidRole()
                {
                    Description = role.Description,
                    Name = role.Name,
                    Spots = role.Spots,
                    IsRandomSignUpRole = role.IsRandomSignUpRole
                });
            }
            foreach (RaidReminder reminder in template.Reminders)
            {
                this.Reminders.Add(new RaidReminder()
                {
                    DiscordServerId = reminder.DiscordServerId,
                    DiscordChannelId = reminder.DiscordChannelId,
                    ReminderTime = reminder.ReminderTime,
                    Message = reminder.Message,
                    Sent = reminder.Sent,
                    Type = reminder.Type,
                    RaidId = reminder.RaidId
                });
            }
            foreach (DiscordRaidMessage message in template.DiscordRaidMessages)
            {
                this.DiscordRaidMessages.Add(new DiscordRaidMessage()
                {
                    RaidId = message.RaidId,
                    DiscordMessageId = message.DiscordMessageId,
                    DiscordChannelId = message.DiscordChannelId,
                    DiscordGuildId = message.DiscordGuildId
                });
            }
        }
    }
}
