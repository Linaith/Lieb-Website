using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;

namespace DiscordBot.CommandHandlers
{
    public class HandlerFunctions
    {
        private readonly HttpService _httpService;

        public HandlerFunctions(HttpService httpService)
        {
            _httpService = httpService;
        }


        public async Task<bool> IsRaidSignUpAllowed(SocketInteraction component, int raidId, string pressedButtonId)
        {
            if(! await _httpService.DoesUserExist(component.User.Id))
            {
                var mb = new ModalBuilder()
                    .WithTitle("Create Account")
                    .WithCustomId($"{Constants.ComponentIds.CREATE_ACCOUNT_MODAL}-{raidId}-{pressedButtonId}")
                    .AddTextInput("Name", Constants.ComponentIds.NAME_TEXT_BOX, placeholder: component.User.Username, required: true, value: component.User.Username)
                    .AddTextInput("Guild Wars 2 Account", Constants.ComponentIds.ACCOUNT_TEXT_BOX, placeholder: "Account.1234", required: true);

                await component.RespondWithModalAsync(mb.Build());
                return false;
            }
            Tuple<bool, string> signUpAllowed = await _httpService.IsSignUpAllowed(raidId, component.User.Id);
            if(!signUpAllowed.Item1)
            {
                await component.RespondAsync(signUpAllowed.Item2, ephemeral: true);
                return false;
            }
            return true;
        }

        public async Task<Tuple<bool, string>> CreateAccount(SocketInteraction interaction, DiscordSocketClient client, string name, string account)
        {
            //create Account
            ApiRaid.Role.User user = new ApiRaid.Role.User()
            {
                UserName = name,
                AccountName = account,
                UserId = interaction.User.Id
            };
            Tuple<bool, string> createAccountResult = await _httpService.CreateAccount(user);
            if(createAccountResult.Item1)
            {
                List<ulong> serverList = await _httpService.GetUserRenameServers();
                await RenameUser(client, interaction.User.Id, name, account, serverList);
            }
            return createAccountResult;
        }


        public static async Task RenameUser(DiscordSocketClient client, ulong userId, string name, string account, List<ulong> serverList)
        {
            string nickname = $"{name} | {account}";
            foreach(ulong serverId in serverList)
            {
                SocketGuild guild = client.Guilds.FirstOrDefault(g => g.Id == serverId);
                if(guild != null)
                {
                    SocketGuildUser user = guild.GetUser(userId);
                    if(user != null)
                    {
                        await user.ModifyAsync(p => p.Nickname = nickname);
                    }
                }
            }
        }
        
        //to avoid error messages because of no response...
        public async Task Respond(SocketInteraction component)
        {
            try
            {
                await component.RespondAsync();
            }
            catch(Discord.Net.HttpException e)
            {

            }
        }
    }
}