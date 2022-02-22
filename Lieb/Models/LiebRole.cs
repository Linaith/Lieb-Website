namespace Lieb.Models
{
    public class LiebRole
    {
        public int LiebRoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
    }
}
