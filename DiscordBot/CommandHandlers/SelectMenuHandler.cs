using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;

namespace DiscordBot.CommandHandlers
{
    public class SelectMenuHandler
    {
        private readonly HttpService _httpService;
        private readonly HandlerFunctions _handlerFunctions;

        public SelectMenuHandler(HttpService httpService)
        {
            _httpService = httpService;
            _handlerFunctions = new HandlerFunctions(_httpService);
        }

        public async Task Handler(SocketMessageComponent component)
        {
            string[] ids = component.Data.CustomId.Split('-');
            switch(ids[0])
            {
                case Constants.ComponentIds.ROLE_SELECT_DROP_DOWN:
                    RoleSelectionMessage.Parameters roleParameters = RoleSelectionMessage.ParseId(component.Data.CustomId);
                    int parsedRoleId = int.Parse(component.Data.Values.First());
                    await _handlerFunctions.SelectAccount(roleParameters.ButtonType, roleParameters.RaidId, component, parsedRoleId, roleParameters.UserIdToSignUp, roleParameters.SignedUpByUserId);
                    break;
                case Constants.ComponentIds.ROLE_SELECT_EXTERNAL_DROP_DOWN:
                    ExternalRoleSelectionMessage.Parameters externalRoleParameters = ExternalRoleSelectionMessage.ParseId(component.Data.CustomId);
                    await component.RespondWithModalAsync(ExternalUserNameModal.buildMessage(externalRoleParameters.RaidId, int.Parse(component.Data.Values.First())));
                    break;
                case Constants.ComponentIds.ACCOUNT_SELECT_DROP_DOWN:
                    AccountSelectionMessage.Parameters accountParameters = AccountSelectionMessage.ParseId(component.Data.CustomId);
                    int accountId = int.Parse(component.Data.Values.First());
                    if(await _handlerFunctions.SignUp(accountParameters.ButtonType, accountParameters.RaidId, accountParameters.RoleId, accountParameters.UserIdToSignUp, accountId, accountParameters.SignedUpByUserId))
                    {
                        await component.UpdateAsync(x => 
                        {
                            x.Content = "successfully signed up";
                            x.Components = null;
                        });
                    }
                    else
                    {
                        await component.UpdateAsync(x => 
                        {
                            x.Content = "signing up failed";
                            x.Components = null;
                        });
                    }
                    break;
            }
        }
    }
}