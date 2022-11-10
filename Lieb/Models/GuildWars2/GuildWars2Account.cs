using System.ComponentModel.DataAnnotations;
using Lieb.Data;

namespace Lieb.Models.GuildWars2
{
    public class GuildWars2Account
    {
        public int GuildWars2AccountId { get; set; }

        public string ApiKey { get; set; } = string.Empty;

        [Required]
        [RegularExpression(Constants.GW2_ACCOUNT_REGEX, ErrorMessage = "Invalid Account Name")]
        public string AccountName { get; set; } = string.Empty;

        public ICollection<Equipped> EquippedBuilds { get; set; } = new List<Equipped>();

    }
}
