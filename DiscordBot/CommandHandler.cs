using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace DiscordBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.ButtonExecuted += MyButtonHandler;
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

        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            string[] ids = component.Data.CustomId.Split('-');
            switch(ids[0])
            {
                case Constants.ComponentIds.SIGN_UP:
                    //await component.RespondAsync($"{component.User.Mention} has clicked the SignUp button!");

                    var mb = new ModalBuilder()
                    .WithTitle("Fav Food")
                    .WithCustomId("food_menu")
                    .AddTextInput("What??", "food_name", placeholder:"Pizza")
                    .AddTextInput("Why??", "food_reason", TextInputStyle.Paragraph, 
                        "Kus it's so tasty");

                    //await component.RespondWithModalAsync(mb.Build());
                    
                    await component.RespondAsync("hi", ephemeral: true);
                break;
                case Constants.ComponentIds.SIGN_OFF:
                    //await component.RespondAsync($"{component.User.Mention} has clicked the SignOff button!");
                break;
            }
        }
    }
}
