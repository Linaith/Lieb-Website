using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;
using DiscordBot;

namespace DiscordBot.CommandHandlers
{
    public class ModalHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly HttpService _httpService;
        private readonly HandlerFunctions _handlerFunctions;

        public ModalHandler(DiscordSocketClient client, HttpService httpService)
        {
            _client = client;
            _httpService = httpService;
            _handlerFunctions = new HandlerFunctions(_httpService);
        }

        public async Task Handler(SocketModal modal)
        {
            string[] ids = modal.Data.CustomId.Split('-');
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            switch(ids[0])
            {
                case Constants.ComponentIds.CREATE_ACCOUNT_MODAL:
                    string name = components.First(x => x.CustomId == Constants.ComponentIds.NAME_TEXT_BOX).Value;
                    string account = components.First(x => x.CustomId == Constants.ComponentIds.ACCOUNT_TEXT_BOX).Value;

                    //create Account
                    Tuple<bool, string> createAccountResult = await _handlerFunctions.CreateAccount(modal, _client, name, account);
                    if(!createAccountResult.Item1)
                    {
                        await modal.RespondAsync(createAccountResult.Item2, ephemeral: true);
                        return;
                    }
                    
                    //sign up
                    if(ids.Length > 2 && int.TryParse(ids[1], out int parsedRaidId) && await _handlerFunctions.IsRaidSignUpAllowed(modal, parsedRaidId, ids[2]))
                    {
                        List<ApiRole> roles = await _httpService.GetRoles(parsedRaidId, modal.User.Id);                    
                        await modal.RespondAsync("Please choose a role.", components: SignUpMessage.buildMessage(roles, parsedRaidId, ids[2], false) , ephemeral: true);
                        return;
                    }
                    await _handlerFunctions.Respond(modal);
                    break;
                case Constants.ComponentIds.SIGN_UP_EXTERNAL_MODAL:
                    string userName = components.First(x => x.CustomId == Constants.ComponentIds.NAME_TEXT_BOX).Value;
                    int raidId = int.Parse(ids[1]);
                    int roleId = int.Parse(ids[2]);
                    ApiSignUp signUpExternal = new ApiSignUp()
                    {
                        raidId = raidId,
                        userName = userName,
                        signedUpByUserId = modal.User.Id,
                        roleId = roleId
                    };
                    await _httpService.SignUp(signUpExternal);
                    await modal.RespondAsync($"signed up {userName}", ephemeral: true);
                    break;
            }
        }
    }
}
