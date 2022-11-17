namespace Lieb.Models
{
    public class DiscordSettings
    {
        public ulong DiscordSettingsId { get; set; }

        public ulong DiscordLogChannel {get; set; }

        public bool ChangeUserNames {get; set;}
    }
}
