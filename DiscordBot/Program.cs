using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Reflection;
using DiscordBot.Services;

namespace DiscordBot
{
    internal class Program
    {
        DiscordSocketClient _client;

        public static Task Main(string[] args) => new Program().MainAsync(args);

        public async Task MainAsync(string[] args)
        {
            var dicordConfig = new DiscordSocketConfig()
            {
                
            };

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton(dicordConfig);
            builder.Services.AddSingleton<DiscordSocketClient>();
            builder.Services.AddSingleton<CommandService>();
            builder.Services.AddSingleton<CommandHandler>();
            builder.Services.AddSingleton<HttpService>();


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpClient(Constants.HTTP_CLIENT_NAME , httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://localhost:7216/");

                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.UserAgent, "HttpRequestsSample");
            }).ConfigurePrimaryHttpMessageHandler(() => {
                            var handler = new HttpClientHandler();
                            if (builder.Environment.IsDevelopment())
                            {
                                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                            }
                            return handler;
                        });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();


            _client = app.Services.GetRequiredService<DiscordSocketClient>();
            var commandHandler = app.Services.GetRequiredService<CommandHandler>();
            await commandHandler.InstallCommandsAsync();

            _client.Log += Log;
            _client.Ready += Client_Ready;

#if DEBUG
            var token = File.ReadAllText("debugtoken.txt");
#else  
            var token = File.ReadAllText("token.txt");
#endif
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();



            app.Run();

            // Block this task until the program is closed.
            await Task.Delay(-1);

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        
        public async Task Client_Ready()
        {
            var guild = _client.GetGuild(666953424734257182);

            var guildCommand = new SlashCommandBuilder()
                .WithName(Constants.SlashCommands.RAID)
                .WithDescription("Raid commands")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(Constants.SlashCommands.USER)
                    .WithDescription("Add or remove users")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName(Constants.SlashCommands.ADD_USER_COMMAND)
                        .WithDescription("Sign up existing user")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(Constants.SlashCommands.OptionNames.RAID_ID, ApplicationCommandOptionType.Integer, "The Id of the Raid, found at the bottom of the raid message", isRequired: true)
                        .AddOption(Constants.SlashCommands.OptionNames.USER, ApplicationCommandOptionType.User, "The user you want to sign up", isRequired: true)
                        )

                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName(Constants.SlashCommands.REMOVE_USER_COMMAND)
                        .WithDescription("Sign off existing user")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(Constants.SlashCommands.OptionNames.RAID_ID, ApplicationCommandOptionType.Integer, "The Id of the Raid, found at the bottom of the raid message", isRequired: true)
                        .AddOption(Constants.SlashCommands.OptionNames.USER, ApplicationCommandOptionType.User, "The user you want to sign off", isRequired: true)
                        )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName(Constants.SlashCommands.ADD_EXTERNAL_USER_COMMAND)
                        .WithDescription("Sign up non existing user")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(Constants.SlashCommands.OptionNames.RAID_ID, ApplicationCommandOptionType.Integer, "The Id of the Raid, found at the bottom of the raid message", isRequired: true)
                        )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName(Constants.SlashCommands.REMOVE_EXTERNAL_USER_COMMAND)
                        .WithDescription("Sign off non existing user")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(Constants.SlashCommands.OptionNames.RAID_ID, ApplicationCommandOptionType.Integer, "The Id of the Raid, found at the bottom of the raid message", isRequired: true)
                        .AddOption(Constants.SlashCommands.OptionNames.USER_NAME, ApplicationCommandOptionType.String, "The user name you want to sign off", isRequired: true)
                        )
                    )
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(Constants.SlashCommands.SEND_MESSAGE_COMMAND)
                    .WithDescription("Send message to all signed up users")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(Constants.SlashCommands.OptionNames.RAID_ID, ApplicationCommandOptionType.Integer, "The Id of the Raid, found at the bottom of the raid message", isRequired: true)
                    .AddOption(Constants.SlashCommands.OptionNames.MESSAGE, ApplicationCommandOptionType.String, "The message you want to send", isRequired: true)
                    );


            try
            {
                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                await guild.CreateApplicationCommandAsync(guildCommand.Build());

                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

    }
}