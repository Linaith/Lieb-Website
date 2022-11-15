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
            List<ApiRole> roles = new List<ApiRole>();
            int parsedRaidId = 0;
            if(ids.Length > 1)
            {
                int.TryParse(ids[1],out parsedRaidId);
            }

            switch(ids[0])
            {
                case Constants.ComponentIds.SIGN_UP_DROP_DOWN:
                    ulong userId = 0;
                    if(ids.Length >= 4)
                    {
                        ulong.TryParse(ids[3],out userId);
                    }
                    await ManageSignUp(ids[2], parsedRaidId, component, userId);
                    await component.RespondAsync("successfully signed up");
                    break;
                case Constants.ComponentIds.SIGN_UP_EXTERNAL_DROP_DOWN:
                    await component.RespondWithModalAsync(CreateUserNameModal(parsedRaidId, int.Parse(component.Data.Values.First())));
                    break;
            }
        }

        private async Task ManageSignUp(string buttonType, int raidId, SocketMessageComponent component, ulong userIdToSignUp)
        {
            if(! int.TryParse(component.Data.Values.First(), out int parsedRoleId)) return;

            ApiSignUp signUp = new ApiSignUp()
            {
                raidId = raidId,
                roleId = parsedRoleId
            };

            if(userIdToSignUp == 0)
            {
                signUp.userId = component.User.Id;
            }
            else
            {
                signUp.userId = userIdToSignUp;
                signUp.signedUpByUserId = component.User.Id;
            }

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

        private Modal CreateUserNameModal(int raidId, int roleId)
        {
            var mb = new ModalBuilder()
                .WithTitle("Create Account")
                .WithCustomId($"{Constants.ComponentIds.SIGN_UP_EXTERNAL_MODAL}-{raidId}-{roleId}")
                .AddTextInput("Name", Constants.ComponentIds.NAME_TEXT_BOX, placeholder: "Name", required: true);

            return mb.Build();
        }
    }
}