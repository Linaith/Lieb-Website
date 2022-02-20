namespace Lieb.Models
{
    public class LiebUser
    {
        public int LiebUserId { get; set; }
        public ulong DiscordUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Pronouns { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public ICollection<GuildWars2Account> GuildWars2Accounts { get; set; } = new List<GuildWars2Account>();
    }
}
