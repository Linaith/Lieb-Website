
namespace SharedClasses.SharedModels
{
    public class ApiRaid
    {
        public string Title { get; set; } = String.Empty;

        public string Description { get; set; } = String.Empty;

        public string Organizer { get; set; } = String.Empty;

        public string Guild { get; set; } = String.Empty;

        public string VoiceChat { get; set; } = String.Empty;

        public int RaidId { get; set; }

        public DateTimeOffset StartTimeUTC { get; set; } = DateTime.Now;

        public DateTimeOffset EndTimeUTC { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();

        public List<DiscordMessage> DisocrdMessages { get; set; } = new List<DiscordMessage>();

        public class DiscordMessage
        {
            public ulong GuildId { get; set; } = 0;

            public ulong ChannelId { get; set; } = 0;

            public ulong MessageId { get; set; } = 0;

            public int WebsiteDatabaseId {get; set; } = 0;
        }

        public class Role
        {
            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public int Spots { get; set; } = 1;

            public List<User> Users { get; set; }


            public class User
            {
                public string UserName { get; set; } = string.Empty;

                public string AccountName { get; set; } = string.Empty;

                public string Status { get; set; } = string.Empty;
            }
        }
    }
}