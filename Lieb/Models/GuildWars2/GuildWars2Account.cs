using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2
{
    public class GuildWars2Account
    {
        public int GuildWars2AccountId { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        [Required]
        [RegularExpression("^[a-zA-z ]{3,27}\\.[0-9]{4}$", ErrorMessage = "Invalid Account Name")]
        public string AccountName { get; set; } = string.Empty;
        public ICollection<Equipped> EquippedBuilds { get; set; } = new List<Equipped>();

    }
}
