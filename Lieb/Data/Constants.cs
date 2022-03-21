namespace Lieb.Data
{
    public static class Constants
    {
        public const string ClaimType = "Role";
        public static readonly int RaidEditPowerLevel = Roles.Moderator.PowerLevel;

        public static class Roles
        {
            public static readonly RoleConstant User = new RoleConstant("user", 20);
            public static readonly RoleConstant RaidLead = new RoleConstant("RaidLead", 40);
            public static readonly RoleConstant Moderator = new RoleConstant("Moderator", 70);
            public static readonly RoleConstant Admin = new RoleConstant("Admin", 100);
        }

        public class RoleConstant
        {
            public readonly string Name;
            public readonly int PowerLevel;

            public RoleConstant(string name, int powerLevel)
            {
                Name = name;
                PowerLevel = powerLevel;
            }
        }

    }
}
