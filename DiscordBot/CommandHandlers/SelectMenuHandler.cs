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
                case Constants.ComponentIds.SIGN_UP_DROP_DOWN:
                    RoleSelectionMessage.Parameters roleParameters = RoleSelectionMessage.ParseId(component.Data.CustomId);
                    ulong userIdToSignUp = component.User.Id;;
                    ulong signedUpByUserId = 0;
                    if(roleParameters.UserIdToSignUp != 0)
                    {
                        userIdToSignUp = roleParameters.UserIdToSignUp;
                        signedUpByUserId = component.User.Id;
                    }
                    await ManageSignUp(roleParameters.ButtonType, roleParameters.RaidId, component, userIdToSignUp, signedUpByUserId);
                    break;
                case Constants.ComponentIds.SIGN_UP_EXTERNAL_DROP_DOWN:
                    ExternalRoleSelectionMessage.Parameters externalRoleParameters = ExternalRoleSelectionMessage.ParseId(component.Data.CustomId);
                    await component.RespondWithModalAsync(ExternalUserNameModal.buildMessage(externalRoleParameters.RaidId, int.Parse(component.Data.Values.First())));
                    break;
                case Constants.ComponentIds.ACCOUNT_SELECT_DROP_DOWN:
                    AccountSelectionMessage.Parameters accountParameters = AccountSelectionMessage.ParseId(component.Data.CustomId);
                    int accountId = int.Parse(component.Data.Values.First());
                    await SignUp(accountParameters.ButtonType, accountParameters.RaidId, accountParameters.RoleId, accountParameters.UserIdToSignUp, accountId, accountParameters.SignedUpByUserId);
                    await component.RespondAsync("successfully signed up", ephemeral: true);
                    break;
            }
        }

        private async Task ManageSignUp(string buttonType, int raidId, SocketMessageComponent component, ulong userIdToSignUp, ulong signedUpByUserId = 0)
        {
            if(! int.TryParse(component.Data.Values.First(), out int parsedRoleId)) return;

            List<ApiGuildWars2Account> accounts = await _httpService.GetSignUpAccounts(userIdToSignUp, raidId);
            if(accounts.Count == 1)
            {
                ApiGuildWars2Account account = accounts.First();
                await SignUp(buttonType, raidId, parsedRoleId, userIdToSignUp, account.GuildWars2AccountId, signedUpByUserId);
                await component.RespondAsync("successfully signed up", ephemeral: true);
            }
            else if(accounts.Count > 1)
            {
                await component.RespondAsync("Please choose an account.", components: AccountSelectionMessage.buildMessage(accounts, raidId, buttonType, parsedRoleId, userIdToSignUp, signedUpByUserId) , ephemeral: true);
            }
            else
            {
                await component.RespondAsync("no suitable Guild Wars 2 account found.", ephemeral: true);
            }
        }

        private async Task SignUp(string buttonType, int raidId, int roleId, ulong userIdToSignUp, int gw2AccountId, ulong signedUpByUserId = 0)
        {
            ApiSignUp signUp = new ApiSignUp()
            {
                raidId = raidId,
                roleId = roleId,
                gw2AccountId = gw2AccountId,
                userId = userIdToSignUp,
                signedUpByUserId = signedUpByUserId
            };

            switch(buttonType)
            {
                case Constants.ComponentIds.SIGN_UP_BUTTON:
                    await _httpService.SignUp(signUp);
                break;
                case Constants.ComponentIds.MAYBE_BUTTON:
                    await _httpService.SignUpMaybe(signUp);
                break;
                case Constants.ComponentIds.BACKUP_BUTTON:
                    await _httpService.SignUpBackup(signUp);
                break;
                case Constants.ComponentIds.FLEX_BUTTON:
                    await _httpService.SignUpFlex(signUp);
                break;
            }
        }
    }
}