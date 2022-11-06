
namespace SharedClasses.SharedModels
{
    public class ApiUserReminder
    {
        public string Message { get; set; }

        public List<ulong> UserIds {get; set;} = new List<ulong>();
    }

    public class ApiChannelReminder
    {
        public string Message { get; set; }

        public ulong DiscordServerId { get; set; }

        public ulong DiscordChannelId { get; set; }
    }
}