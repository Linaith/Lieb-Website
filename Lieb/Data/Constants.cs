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
    }
}
