using Discord.WebSocket;
using DiscordBot.Messages;
using DiscordBot.Services;
using SharedClasses.SharedModels;

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
                case Constants.ComponentIds.REMOVE_USER_DROP_DOWN:
                    RemoveUserMessage.Parameters removeUserParameters = RemoveUserMessage.ParseId(component.Data.CustomId);
                    await RemoveUser(component, removeUserParameters);
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
                case Constants.ComponentIds.POLL_DROP_DOWN:
                    PollMessage.Parameters pollParameters = PollMessage.ParseId(component.Data.CustomId);
                    ApiPollAnswer answer = new ApiPollAnswer()
                    {
                        Answer = string.Empty,
                        OptionId = int.Parse(component.Data.Values.First()),
                        PollId = pollParameters.PollId,
                        UserId = component.User.Id
                    };
                    await _httpService.AnswerPoll(answer);
                    await component.RespondAsync("Answer sent.", ephemeral: true);
                    break;
            }
        }

        private async Task RemoveUser(SocketMessageComponent component, RemoveUserMessage.Parameters removeUserParameters)
        {
            ApiSignUp signOff = new ApiSignUp()
            {
                raidId = removeUserParameters.RaidId,
                signedUpByUserId = removeUserParameters.SignedUpByUserId
            };
            if (ulong.TryParse(component.Data.Values.First(), out ulong userId))
            {
                signOff.userId = userId;
            }
            else
            {
                signOff.userName = component.Data.Values.First();
            }
            await _httpService.SignOff(signOff);
            await _handlerFunctions.UpdateOrRespond(component, $"signed off {component.Data.Values.First()}", null, true);
        }
    }
}