using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;
using DiscordBot.CommandHandlers;

namespace DiscordBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly HttpService _httpService;
        private readonly SlashCommandHandler _slashCommandHandler;
        private readonly ButtonHandler _buttonHandler;
        private readonly SelectMenuHandler _selectMenuHandler;
        private readonly ModalHandler _modalHandler;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands, HttpService httpService)
        {
            _commands = commands;
            _client = client;
            _httpService = httpService;
            _slashCommandHandler = new SlashCommandHandler(_client, _httpService);
            _buttonHandler = new ButtonHandler(_httpService);
            _selectMenuHandler = new SelectMenuHandler(_httpService);
            _modalHandler = new ModalHandler(_client, _httpService);
        }

        public async Task InstallCommandsAsync()
        {
            _client.SlashCommandExecuted += _slashCommandHandler.Handler;
            _client.ButtonExecuted += _buttonHandler.Handler;
            _client.SelectMenuExecuted += _selectMenuHandler.Handler;
            _client.ModalSubmitted += _modalHandler.Handler;
        }
    }
}
