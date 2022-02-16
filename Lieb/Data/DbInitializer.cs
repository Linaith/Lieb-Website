using Lieb.Models;

namespace Lieb.Data
{
    public class DbInitializer
    {
        public static void Initialize(LiebContext context)
        {
            // Look for any students.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new LiebUser[]
            {
                new LiebUser{DiscordUserId=0, Name="Sarah",Birthday=DateTime.Parse("1992-01-15")},
                new LiebUser{DiscordUserId=0, Name="Lisa",Birthday=DateTime.Parse("1991-02-15")},
                new LiebUser{DiscordUserId=0, Name="Simon",Birthday=DateTime.Parse("2019-09-01")}
            };

            context.Users.AddRange(users);
            context.SaveChanges();

        }
    }
}
