using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;

namespace DiscordBot.CommandHandlers
{
    public class ButtonHandler
    {
        private readonly HttpService _httpService;
        private readonly HandlerFunctions _handlerFunctions;

        public ButtonHandler(HttpService httpService)
        {
            _httpService = httpService;
            _handlerFunctions = new HandlerFunctions(_httpService);
        }

        public async Task Handler(SocketMessageComponent component)
        {
            string[] ids = component.Data.CustomId.Split('-');
            List<ApiRole> roles = new List<ApiRole>();
            int parsedRaidId = 0;
            if(ids.Length > 1)
            {
                int.TryParse(ids[1],out parsedRaidId);
            }
            switch(ids[0])
            {
                case Constants.ComponentIds.SIGN_UP_BUTTON:
                    if(await _handlerFunctions.IsRaidSignUpAllowed(component, parsedRaidId, ids[0]))
                    {
                        roles = await _httpService.GetRoles(parsedRaidId, component.User.Id);                    
                        await component.RespondAsync("Please choose a role.", components: SignUpMessage.buildMessage(roles, parsedRaidId, ids[0], false) , ephemeral: true);
                    }
                break;
                case Constants.ComponentIds.MAYBE_BUTTON:
                case Constants.ComponentIds.BACKUP_BUTTON:
                case Constants.ComponentIds.FLEX_BUTTON:
                    if(await _handlerFunctions.IsRaidSignUpAllowed(component, parsedRaidId, ids[0]))
                    {
                        roles = await _httpService.GetRoles(parsedRaidId, component.User.Id);                    
                        await component.RespondAsync("Please choose a role.", components: SignUpMessage.buildMessage(roles, parsedRaidId, ids[0], true) , ephemeral: true);
                    }
                break;
                case Constants.ComponentIds.SIGN_OFF_BUTTON:
                    ApiSignUp signOff = new ApiSignUp()
                    {
                        raidId = parsedRaidId,
                        userId = component.User.Id
                    };
                    await _httpService.SignOff(signOff);
                    await _handlerFunctions.Respond(component);
                break;
            }
        }

    }
}