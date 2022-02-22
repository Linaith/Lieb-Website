using Lieb.Models;

namespace Lieb.Data
{
    public class DbInitializer
    {
        public static void Initialize(LiebContext context)
        {
            // Look for any students.
            if (context.LiebUsers.Any())
            {
                return;   // DB has been seeded
            }

            UserRole admin = new UserRole();
            admin.RoleName = Constants.Roles.Admin;
            UserRole guildLead = new UserRole();
            guildLead.RoleName = Constants.Roles.GuildLead;
            UserRole member = new UserRole();
            member.RoleName = Constants.Roles.User;


            var users = new LiebUser[]
            {
                new LiebUser{DiscordUserId=0, Name="Sarah",Birthday=DateTime.Parse("1992-01-15"), Roles=new List<UserRole>(){admin}},
                new LiebUser{DiscordUserId=1, Name="Lisa",Birthday=DateTime.Parse("1991-02-15"), Roles=new List<UserRole>(){guildLead} },
                new LiebUser{DiscordUserId=2, Name="Simon",Birthday=DateTime.Parse("2019-09-01"), Roles=new List<UserRole>(){member}}
            };

            context.LiebUsers.AddRange(users);
            context.SaveChanges();

        }
    }
}
