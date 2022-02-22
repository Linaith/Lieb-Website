using Lieb.Models.GuildWars2;

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
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}
