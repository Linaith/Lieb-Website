
namespace SharedClasses.SharedModels
{
    public class DiscordServer
    {
        public string Name {get; set;}
        public ulong Id {get; set;}
        public List<DiscordChannel> Channels {get; set;} = new List<DiscordChannel>();
    }

    public class DiscordChannel
    {
        public string Name {get; set;}
        public ulong Id {get; set;}
    }
}