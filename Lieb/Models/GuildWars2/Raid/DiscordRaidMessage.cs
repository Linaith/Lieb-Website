using System.Text.Json.Serialization;

namespace Lieb.Models.GuildWars2.Raid
{
    public class DiscordRaidMessage
    {
        public int DiscordRaidMessageId { get; set; }

        public ulong DiscordMessageId { get; set; }

        public ulong DiscordChannelId { get; set; }

        public ulong DiscordGuildId { get; set; }
    }
}
