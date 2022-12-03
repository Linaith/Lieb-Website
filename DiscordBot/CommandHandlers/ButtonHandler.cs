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
            switch(ids[0])
            {
                case Constants.ComponentIds.SIGN_UP_BUTTON:
                case Constants.ComponentIds.MAYBE_BUTTON:
                    RaidMessage.ButtonParameters signUpParameters = RaidMessage.ParseButtonId(component.Data.CustomId);
                    await _handlerFunctions.SelectRole(component, signUpParameters.RaidId, signUpParameters.ButtonType, false, component.User.Id, 0);
                    break;
                case Constants.ComponentIds.BACKUP_BUTTON:
                    RaidMessage.ButtonParameters backupParameters = RaidMessage.ParseButtonId(component.Data.CustomId);
                    await _handlerFunctions.SelectRole(component, backupParameters.RaidId, backupParameters.ButtonType, true, component.User.Id, 0);
                    break;
                case Constants.ComponentIds.FLEX_BUTTON:
                    RaidMessage.ButtonParameters flexParameters = RaidMessage.ParseButtonId(component.Data.CustomId);
                    if(await _httpService.IsUserSignedUp(flexParameters.RaidId, component.User.Id))
                    {
                        await _handlerFunctions.SelectRole(component, flexParameters.RaidId, flexParameters.ButtonType, true, component.User.Id, 0);
                    }
                    else
                    {
                        await component.RespondAsync("Flex sign up is only allowed if you are already signed up.", ephemeral: true);
                    }
                    break;
                case Constants.ComponentIds.SIGN_OFF_BUTTON:
                    RaidMessage.ButtonParameters signOffParameters = RaidMessage.ParseButtonId(component.Data.CustomId);
                    ApiSignUp signOff = new ApiSignUp()
                    {
                        raidId = signOffParameters.RaidId,
                        userId = component.User.Id
                    };
                    await _httpService.SignOff(signOff);
                    await component.RespondAsync("Signed Off", ephemeral: true);
                break;
            }
        }
    }
}