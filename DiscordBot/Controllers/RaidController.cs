using Microsoft.AspNetCore.Mvc;
using SharedClasses.SharedModels;
using DiscordBot.Messages;
using Discord;
using Discord.WebSocket;
using System.Linq;

namespace DiscordBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RaidController : ControllerBase
    {
        
        DiscordSocketClient _client;

        public RaidController(DiscordSocketClient client)
        {
            _client = client;
        }

/*{"Title":"Testraid","Description":"This is a test raidnwith multiple lines?","Organizer":"Sarah","Guild":"LIEB","VoiceChat":"ts.lieb.games","RaidId":4,"StartTimeUTC":"2022-11-13T10:24:58.3955622+00:00","EndTimeUTC":"2022-11-14T12:24:58.3955622+00:00","Roles":[{"Name":"Random","Description":"RandomWithBoons","Spots":10,"Users":[]}],"DisocrdMessages":[{"GuildId":666953424734257182,"ChannelId":666954070388637697,"MessageId":0,"WebsiteDatabaseId":2}]}*/
        [HttpPost]
        [Route("[action]")]
        public async Task<ApiRaid> PostRaidMessage(ApiRaid raid)
        {
            RaidMessage message = new RaidMessage(_client);
            return await message.PostRaidMessage(raid);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task DeleteRaidMessage(IEnumerable<ApiRaid.DiscordMessage> messages)
        {
            RaidMessage message = new RaidMessage(_client);
            await message.DeleteRaidMessage(messages);
        }

        [HttpGet]
        [Route("[action]")]
        public List<DiscordServer> GetServers()
        {
            List<DiscordServer> servers = new List<DiscordServer>();
            foreach(var guild in _client.Guilds)
            {
                DiscordServer server = new DiscordServer(){
                    Name = guild.Name,
                    Id = guild.Id
                };

                foreach(var channel in guild.Channels)
                {
                    if( channel is SocketTextChannel && channel.Users.Where(u => u.Id == _client.CurrentUser.Id).Any())
                    {
                        server.Channels.Add(new DiscordChannel(){
                            Name = channel.Name,
                            Id = channel.Id
                        });
                    }
                }
                servers.Add(server);
            }
            return servers;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SendUserReminder(ApiUserReminder reminder)
        {
            foreach(ulong userId in reminder.UserIds)
            {
                var user = await _client.GetUserAsync(userId);
                if(user != null)
                {
                    await user.SendMessageAsync(reminder.Message);
                }
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SendChannelReminder(ApiChannelReminder reminder)
        {
            var channel = _client.GetGuild(reminder.DiscordServerId).GetChannel(reminder.DiscordChannelId);
            if (channel != null && channel is IMessageChannel)
            {
                IMessageChannel messageChannel = channel as IMessageChannel;
                await messageChannel.SendMessageAsync(reminder.Message);
            }
        }
    }
}