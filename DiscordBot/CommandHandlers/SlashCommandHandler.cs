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
    public class SlashCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly HttpService _httpService;
        private readonly HandlerFunctions _handlerFunctions;

        public SlashCommandHandler(DiscordSocketClient client, HttpService httpService)
        {
            _client = client;
            _httpService = httpService;
            _handlerFunctions = new HandlerFunctions(_httpService);
        }

        public async Task Handler(SocketSlashCommand command)
        {
            Tuple<bool, string> commandExecutionAllowed = await _httpService.IsSlashCommandAllowed(command.User.Id, command.Data.Name);
            if(commandExecutionAllowed.Item1)
            {
                switch (command.Data.Name)
                {
                    case Constants.SlashCommands.RAID:
                        await HandleRaidCommands(command);
                        break;
                }
            }
            else
            {
                await command.RespondAsync(commandExecutionAllowed.Item2, ephemeral:true);
            }
        }

        private async Task HandleRaidCommands(SocketSlashCommand command)
        {
            switch (command.Data.Options.First().Name)
            {
                case Constants.SlashCommands.USER:
                    await HandleUserCommands(command);
                    break;
                case Constants.SlashCommands.SEND_MESSAGE_COMMAND:
                    int raidId = (int)(long)command.Data.Options.First().Options?.FirstOrDefault(o => o.Name == Constants.SlashCommands.OptionNames.RAID_ID).Value;
                    string message = (string)command.Data.Options.First().Options?.FirstOrDefault(o => o.Name == Constants.SlashCommands.OptionNames.MESSAGE).Value;
                    await command.DeferAsync(ephemeral:true);
                    await SendMessages(message, raidId);
                    await command.ModifyOriginalResponseAsync(r => r.Content = $"message sent");
                    break;
            }
        }

        private async Task HandleUserCommands(SocketSlashCommand command)
        {
            int raidId = (int)(long)command.Data.Options.First().Options.First().Options?.FirstOrDefault(o => o.Name == Constants.SlashCommands.OptionNames.RAID_ID).Value;
            string userName = string.Empty;
            IUser user;
            switch (command.Data.Options.First().Options.First().Name)
            {
                case Constants.SlashCommands.ADD_USER_COMMAND:
                    user = (IUser)command.Data.Options.First().Options.First().Options?.FirstOrDefault(o => o.Name == Constants.SlashCommands.OptionNames.USER).Value;
                    await SignUpUser(command, user, raidId);
                    await command.RespondAsync($"signed up {user.Username}", ephemeral:true);
                    break;
                case Constants.SlashCommands.ADD_EXTERNAL_USER_COMMAND:
                    await SignUpExternalUser(command, raidId);
                    break;
                case Constants.SlashCommands.REMOVE_USER_COMMAND:
                    user = (IUser)command.Data.Options.First().Options.First().Options?.FirstOrDefault(o => o.Name == Constants.SlashCommands.OptionNames.USER).Value;
                    ApiSignUp signOff = new ApiSignUp()
                    {
                        raidId = raidId,
                        userId = user.Id,
                        signedUpByUserId = command.User.Id
                    };
                    await _httpService.SignOff(signOff);
                    await command.RespondAsync($"signed off {user.Username}", ephemeral:true);
                    break;
                case Constants.SlashCommands.REMOVE_EXTERNAL_USER_COMMAND:
                    userName = (string)command.Data.Options.First().Options.First().Options?.FirstOrDefault(o => o.Name == Constants.SlashCommands.OptionNames.USER_NAME).Value;
                    ApiSignUp signOffExternal = new ApiSignUp()
                    {
                        raidId = raidId,
                        userName = userName,
                        signedUpByUserId = command.User.Id
                    };
                    await _httpService.SignOff(signOffExternal);
                    await command.RespondAsync($"signed off {userName}", ephemeral:true);
                    break;
                case Constants.SlashCommands.REMOVE_USER_DROPDOWN_COMMAND:
                    ApiRaid raid = await _httpService.GetRaid(raidId);
                    await command.RespondAsync("Which user do you want to sign off?", components: RemoveUserMessage.buildMessage(raid, command.User.Id), ephemeral: true);
                    break;
            }
        }

        private async Task SignUpUser(SocketSlashCommand command, IUser user, int raidId)
        { 
            if(await _httpService.DoesUserExist(user.Id))
            {                
                Tuple<bool, string> signUpAllowed = await _httpService.IsSignUpAllowed(raidId, user.Id, true);
                if(!signUpAllowed.Item1)
                {
                    await command.RespondAsync(signUpAllowed.Item2, ephemeral: true);
                    return;
                }
                await _handlerFunctions.SelectRole(command, raidId, Constants.ComponentIds.SIGN_UP_BUTTON, false, user.Id, command.User.Id); 
            }
            else
            {
                await command.RespondAsync("This user has no account, please use sign up external", ephemeral: true);
            }
        }

        private async Task SignUpExternalUser(SocketSlashCommand command, int raidId)
        {  
            List<ApiRole> roles = await _httpService.GetRoles(raidId, uint.MaxValue);    
            Tuple<bool, string> signUpAllowed = await _httpService.IsExternalSignUpAllowed(raidId);
            if(!signUpAllowed.Item1)
            {
                await command.RespondAsync(signUpAllowed.Item2, ephemeral: true);
                return;
            }
            await command.RespondAsync("Please choose a role.", components: ExternalRoleSelectionMessage.buildMessage(roles, raidId) , ephemeral: true);
        }

        private async Task SendMessages(string message, int raidId)
        {
            ApiRaid raid  = await _httpService.GetRaid(raidId);
            HashSet<ulong> users = new HashSet<ulong>();
            foreach(ApiRaid.Role role in raid.Roles)
            {
                foreach(ulong userId in role.Users.Select(u => u.UserId).ToHashSet())
                {
                    users.Add(userId);
                }
            }
            foreach(ulong userId in users)
            {
                if(userId > 100)
                {
                    IUser discordUser = await _client.GetUserAsync(userId);
                    await discordUser.SendMessageAsync($"{raid.Title}: {message}");
                }
            }
        }
    }
}