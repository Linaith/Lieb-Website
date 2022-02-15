namespace Lieb.Models
{
    public class User
    {
        public int UserId { get; set; }
        public ulong DiscordUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public ICollection<GuildWars2Account> GuildWars2Accounts { get; set; } = new List<GuildWars2Account>();
    }
}
