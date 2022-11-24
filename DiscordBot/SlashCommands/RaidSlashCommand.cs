using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Services;
using SharedClasses.SharedModels;
using DiscordBot.Messages;

namespace DiscordBot.SlashCommands
{
    public class RaidSlashCommand
    {
        public static SlashCommandBuilder CreateRaidCommand()
        {
            return new SlashCommandBuilder()
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
        }
    }
}