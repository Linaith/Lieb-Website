namespace Lieb.Models.GuildWars2.Raid
{
    public class DiscordRaidMessage
    {
        public int DiscordRaidMessageId { get; set; }

        public int RaidId { get; set; }

        public Raid Raid { get; set; }

        public ulong DiscordMessageId { get; set; }

        public ulong DiscordChannelId { get; set; }

        public ulong DiscordGuildId { get; set; }
    }
}
