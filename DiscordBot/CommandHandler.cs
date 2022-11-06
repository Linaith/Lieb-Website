using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;

namespace DiscordBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly HttpService _httpService;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands, HttpService httpService)
        {
            _commands = commands;
            _client = client;
            _httpService = httpService;
        }

        public async Task InstallCommandsAsync()
        {
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.ButtonExecuted += ButtonHandler;
            _client.SelectMenuExecuted += SelectMenuHandler;
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case Constants.SlashCommands.FIRST_COMMAND:
                    await HandleFirstCommand(command);
                    break;
            }
            await command.RespondAsync($"You executed {command.Data.Name}");
        }


        private async Task HandleFirstCommand(SocketSlashCommand command)
        {
            // We need to extract the user parameter from the command. since we only have one option and it's required, we can just use the first option.
            var guildUser = (SocketGuildUser)command.Data.Options.First().Value;

            // We remove the everyone role and select the mention of each role.
            var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
                .WithTitle("Roles")
                .WithDescription(roleList)
                .WithColor(Color.Green)
                .WithCurrentTimestamp();

            // Now, Let's respond with the embed.
            await command.RespondAsync(embed: embedBuiler.Build());
        }

        public async Task ButtonHandler(SocketMessageComponent component)
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
                    roles = await _httpService.GetRoles(parsedRaidId, component.User.Id);                    
                    await component.RespondAsync("Please choose a role.", components: SignUpMessage.buildMessage(roles, parsedRaidId, ids[0], false) , ephemeral: true);
                break;
                case Constants.ComponentIds.MAYBE_BUTTON:
                case Constants.ComponentIds.BACKUP_BUTTON:
                case Constants.ComponentIds.FLEX_BUTTON:
                    roles = await _httpService.GetRoles(parsedRaidId, component.User.Id);                    
                    await component.RespondAsync("Please choose a role.", components: SignUpMessage.buildMessage(roles, parsedRaidId, ids[0], true) , ephemeral: true);
                break;
                case Constants.ComponentIds.SIGN_OFF_BUTTON:
                    ApiSignUp signOff = new ApiSignUp()
                    {
                        raidId = parsedRaidId,
                        userId = component.User.Id
                    };
                    await _httpService.SignOff(signOff);
                    await Respond(component);
                break;
            }
        }

        public async Task SelectMenuHandler(SocketMessageComponent component)
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
                    await ManageSignUp(ids[2], parsedRaidId, component);
                    await Respond(component);
                break;
            }
        }

        private async Task ManageSignUp(string buttonType, int raidId, SocketMessageComponent component)
        {
            if(! int.TryParse(component.Data.Values.First(), out int parsedRoleId)) return;

            ApiSignUp signUp = new ApiSignUp()
            {
                raidId = raidId,
                userId = component.User.Id,
                roleId = parsedRoleId
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
        private async Task Respond(SocketMessageComponent component)
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
