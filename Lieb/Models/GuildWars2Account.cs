namespace Lieb.Models
{
    public class GuildWars2Account
    {
        public int GuildWars2AccountId { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public ICollection<Equipped> EquippedRoles { get; set; } = new List<Equipped>();

    }
}
