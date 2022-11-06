using System.ComponentModel.DataAnnotations;

namespace Lieb.Models.GuildWars2.Raid
{
    public class RaidRole
    {
        public int RaidRoleId { get; set; }
        public int Spots { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "Name too long (40 character limit).")]
        public string Name { get; set; } = String.Empty;

        [Required]
        [StringLength(200, ErrorMessage = "Description too long (200 character limit).")]
        public string Description { get; set; } = String.Empty;

        public bool IsRandomSignUpRole { get; set; } = false;
    }
}
