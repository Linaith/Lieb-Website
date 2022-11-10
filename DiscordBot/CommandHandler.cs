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
            _client.ModalSubmitted += ModalHandler;
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
                    if(await IsRaidSignUpAllowed(component, parsedRaidId, ids[0]))
                    {
                        roles = await _httpService.GetRoles(parsedRaidId, component.User.Id);                    
                        await component.RespondAsync("Please choose a role.", components: SignUpMessage.buildMessage(roles, parsedRaidId, ids[0], false) , ephemeral: true);
                    }
                break;
                case Constants.ComponentIds.MAYBE_BUTTON:
                case Constants.ComponentIds.BACKUP_BUTTON:
                case Constants.ComponentIds.FLEX_BUTTON:
                    if(await IsRaidSignUpAllowed(component, parsedRaidId, ids[0]))
                    {
                        roles = await _httpService.GetRoles(parsedRaidId, component.User.Id);                    
                        await component.RespondAsync("Please choose a role.", components: SignUpMessage.buildMessage(roles, parsedRaidId, ids[0], true) , ephemeral: true);
                    }
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

        public async Task ModalHandler(SocketModal modal)
        {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            string name = components.First(x => x.CustomId == Constants.ComponentIds.NAME_TEXT_BOX).Value;
            string account = components.First(x => x.CustomId == Constants.ComponentIds.ACCOUNT_TEXT_BOX).Value;

            //create Account
            ApiRaid.Role.User user = new ApiRaid.Role.User()
            {
                UserName = name,
                AccountName = account,
                UserId = modal.User.Id
            };
            Tuple<bool, string> createAccountResult = await _httpService.CreateAccount(user);
            if(!createAccountResult.Item1)
            {
                await modal.RespondAsync(createAccountResult.Item2, ephemeral: true);
                return;
            }
            
            //sign up
            string[] ids = modal.Data.CustomId.Split('-');
            if(ids.Length > 2 && int.TryParse(ids[1], out int parsedRaidId) && await IsRaidSignUpAllowed(modal, parsedRaidId, ids[2]))
            {
                List<ApiRole> roles = await _httpService.GetRoles(parsedRaidId, modal.User.Id);                    
                await modal.RespondAsync("Please choose a role.", components: SignUpMessage.buildMessage(roles, parsedRaidId, ids[2], false) , ephemeral: true);
                return;
            }
            await Respond(modal);
        }

        //to avoid error messages because of no response...
        private async Task Respond(SocketInteraction component)
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
