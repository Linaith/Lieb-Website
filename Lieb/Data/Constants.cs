namespace Lieb.Data
{
    public static class Constants
    {


        public const string ClaimType = "Role";
        public static class Roles
        {
            public const string User = "User";
            public const string RaidLead = "RaidLead";
            public const string GuildLead = "GuildLead";
            public const string Admin = "Admin";

            public static List<string> GetAllRoles()
            {
                return typeof(Roles).GetFields().Select(f => f.GetValue(f)).Cast<string>().ToList();
            }
        }

        public static class RoleLevels
        {
            public const int UserLevel = 20;
            public const int RaidLeadLevel = 55;
            public const int GuildLeadLevel = 65;
            public const int AdminLevel = 80;
        }
    }
}
