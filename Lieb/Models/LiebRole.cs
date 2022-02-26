using System.ComponentModel.DataAnnotations;

namespace Lieb.Models
{
    public class LiebRole
    {
        public int LiebRoleId { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "RoleName too long (40 character limit).")]
        public string RoleName { get; set; } = string.Empty;

        public ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
    }
}
