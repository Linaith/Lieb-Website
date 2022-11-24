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
            var dicordConfig = new DiscordSocketConfig();

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
                httpClient.BaseAddress = new Uri(builder.Configuration["HttpClients:LiebWebsite"]);

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
            //test Discord
            var guild = _client.GetGuild(666953424734257182);

            var guildCommand = SlashCommands.RaidSlashCommand.CreateRaidCommand();

            try
            {
                //create guild commands for testing
                //await guild.CreateApplicationCommandAsync(guildCommand.Build());
                
                //delete guild commands after testing
                //await guild.DeleteApplicationCommandsAsync();

                //create global command, only do this once per command. Uncomment and execute before publishing.
                //await _client.CreateGlobalApplicationCommandAsync(guildCommand.Build());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }
}