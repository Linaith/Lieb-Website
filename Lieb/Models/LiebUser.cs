using Lieb.Models.GuildWars2;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lieb.Models
{
    public class LiebUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "Name too long (40 character limit).")]
        public string Name { get; set; } = string.Empty;

        [StringLength(60, ErrorMessage = "Pronouns too long (60 character limit).")]
        public string Pronouns { get; set; } = string.Empty;

        public DateTime? Birthday { get; set; }
        public DateTime? BannedUntil { get; set; }
        public int MainGW2Account { get; set; }
        public ICollection<GuildWars2Account> GuildWars2Accounts { get; set; } = new List<GuildWars2Account>();
        public ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
    }
}
