using System.ComponentModel.DataAnnotations;

namespace Lieb.Models
{
    public class LiebRole
    {
        public int LiebRoleId { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "RoleName too long (40 character limit).")]
        public string RoleName { get; set; } = string.Empty;

        public bool IsSystemRole { get; set; } = false;

        public int Level { get; set; } = 20;

        public int LevelToAssign { get; set; } = 30;

        public ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
    }
}
