namespace Lieb.Models
{
    public class RoleAssignment
    {
        public int RoleAssignmentId { get; set; }


        public int LiebRoleId { get; set; }
        public int LiebUserId { get; set; }
        public LiebRole LiebRole { get; set; }
        public LiebUser LiebUser { get; set; }
    }
}
