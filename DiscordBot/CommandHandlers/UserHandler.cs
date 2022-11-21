using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;

namespace DiscordBot.CommandHandlers
{
    public class UserHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly HttpService _httpService;
        private readonly HandlerFunctions _handlerFunctions;

        public UserHandler(DiscordSocketClient client,HttpService httpService)
        {
            _client = client;
            _httpService = httpService;
            _handlerFunctions = new HandlerFunctions(_httpService);
        }

        public async Task HandleUserJoined(SocketGuildUser user)
        {
            if((await _httpService.GetUserRenameServers()).Contains(user.Guild.Id))
            {
                if(await _httpService.DoesUserExist(user.Id))
                {
                    ApiRaid.Role.User apiUser = await _httpService.GetUser(user.Id);
                    await HandlerFunctions.RenameUser(_client, user.Id, apiUser.UserName, apiUser.AccountName, new List<ulong>(){user.Guild.Id});
                }
            }
        }

    }
}