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
                await component.RespondWithModalAsync(CreateAccountModal.buildMessage(raidId, pressedButtonId, component.User.Username));
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

        public async Task SelectRole(SocketInteraction component, int raidId, string buttonType, bool allRoles, ulong userIdToSignUp, ulong signedUpByUserId)
        {
            if(await IsRaidSignUpAllowed(component, raidId, buttonType))
            {
                List<ApiRole> roles = new List<ApiRole>();
                roles = await _httpService.GetRoles(raidId, userIdToSignUp);       
                if(roles.Count > 1)
                {             
                    await component.RespondAsync("Please choose a role.", components: RoleSelectionMessage.buildMessage(roles, raidId, buttonType, allRoles, userIdToSignUp, signedUpByUserId) , ephemeral: true);
                }
                else if(roles.Count == 1)
                {
                    await SelectAccount(buttonType, raidId, component, roles.First().roleId, userIdToSignUp, signedUpByUserId);
                }
                else
                {
                    await component.RespondAsync("no role found.", ephemeral: true);
                }
            }
        }

        public async Task SelectAccount(string buttonType, int raidId, SocketInteraction component, int roleId, ulong userIdToSignUp, ulong signedUpByUserId)
        {

            List<ApiGuildWars2Account> accounts = await _httpService.GetSignUpAccounts(userIdToSignUp, raidId);
            if(accounts.Count == 1)
            {
                ApiGuildWars2Account account = accounts.First();
                await SignUp(buttonType, raidId, roleId, userIdToSignUp, account.GuildWars2AccountId, signedUpByUserId);
                await component.RespondAsync("successfully signed up", ephemeral: true);
            }
            else if(accounts.Count > 1)
            {
                await component.RespondAsync("Please choose an account.", components: AccountSelectionMessage.buildMessage(accounts, raidId, buttonType, roleId, userIdToSignUp, signedUpByUserId) , ephemeral: true);
            }
            else
            {
                await component.RespondAsync("no suitable Guild Wars 2 account found.", ephemeral: true);
            }
        }

        public async Task SignUp(string buttonType, int raidId, int roleId, ulong userIdToSignUp, int gw2AccountId, ulong signedUpByUserId)
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