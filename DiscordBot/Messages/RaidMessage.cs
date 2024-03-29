﻿using Discord;
using Discord.WebSocket;
using System;
using System.ComponentModel.DataAnnotations;
using SharedClasses.SharedModels;

namespace DiscordBot.Messages
{
    public class RaidMessage
    {
        DiscordSocketClient _client;

        public RaidMessage(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task DeleteRaidMessage(IEnumerable<ApiRaid.DiscordMessage> messages)
        {
            foreach (ApiRaid.DiscordMessage message in messages)
            {
                var channel = _client.GetGuild(message.GuildId).GetChannel(message.ChannelId);
                if (channel != null && channel is IMessageChannel)
                {
                    IMessageChannel messageChannel = channel as IMessageChannel;
                    if (message.MessageId != 0)
                    {
                        await messageChannel.DeleteMessageAsync(message.MessageId);
                    }
                }
            }
        }

        public async Task<ApiRaid> PostRaidMessage(ApiRaid raid)
        {
            var builder = new ComponentBuilder()
                .WithButton("SignUp", $"{Constants.ComponentIds.SIGN_UP_BUTTON}-{raid.RaidId.ToString()}", ButtonStyle.Success)
                .WithButton("Maybe", $"{Constants.ComponentIds.MAYBE_BUTTON}-{raid.RaidId.ToString()}", ButtonStyle.Secondary)
                .WithButton("Backup", $"{Constants.ComponentIds.BACKUP_BUTTON}-{raid.RaidId.ToString()}", ButtonStyle.Secondary);
                if(raid.AllowFlexRole)
                {
                    builder.WithButton("Flex", $"{Constants.ComponentIds.FLEX_BUTTON}-{raid.RaidId.ToString()}", ButtonStyle.Secondary);
                }
                builder.WithButton("SignOff", $"{Constants.ComponentIds.SIGN_OFF_BUTTON}-{raid.RaidId.ToString()}", ButtonStyle.Danger);

            MessageComponent components = builder.Build();
            Embed raidMessage = CreateRaidMessage(raid);

            foreach (ApiRaid.DiscordMessage message in raid.DisocrdMessages)
            {
                try
                {
                    var channel = _client.GetGuild(message.GuildId).GetChannel(message.ChannelId);
                    if (channel != null && channel is IMessageChannel)
                    {
                        IMessageChannel messageChannel = channel as IMessageChannel;
                        if (message.MessageId != 0)
                        {
                            MessageProperties properties  = new MessageProperties()
                            {
                                Embed = raidMessage,
                                Components = components
                            };
                            IUserMessage discordMessage = (IUserMessage)await messageChannel.GetMessageAsync(message.MessageId);
                            await discordMessage.ModifyAsync(msg => msg.Embed = raidMessage);
                        }
                        else
                        {
                            IUserMessage sentMessage = await messageChannel.SendMessageAsync(embed: raidMessage, components: components);
                            message.MessageId = sentMessage.Id;
                        }
                    }
                }
                catch {}
            }
            return raid;
        }

        private Embed CreateRaidMessage(ApiRaid raid)
        {
            var embed = new EmbedBuilder()
            {
                Title = raid.Title,
                Description = raid.Description,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"RaidId: {raid.RaidId}"
                }
            };
            AddMessageDetails(raid, ref embed);
            AddMessageRoles(raid, ref embed);

            return embed.Build();
        }

        private void AddMessageDetails(ApiRaid raid, ref EmbedBuilder embed)
        {
            //embed.AddField("Date", $"{raid.StartTimeUTC.ToLocalTime().DateTime.ToLongDateString()}");
            //embed.AddField("Time", $"from: {raid.StartTimeUTC.ToLocalTime().DateTime.ToShortTimeString()}  to: {raid.EndTimeUTC.ToLocalTime().DateTime.ToShortTimeString()}");
            embed.AddField("Start ", $"<t:{raid.StartTimeUTC.ToUnixTimeSeconds()}:F>");
            embed.AddField("End", $"<t:{raid.EndTimeUTC.ToUnixTimeSeconds()}:F>");
            embed.AddField("Organisator", raid.Organizer, true);
            embed.AddField("Guild", raid.Guild, true);
            embed.AddField("Voice chat", raid.VoiceChat, true);
        }

        private void AddMessageRoles(ApiRaid raid, ref EmbedBuilder embed)
        {
            Dictionary<string, string> fieldList = new Dictionary<string, string>();

            embed.AddField("Signed up", $"({raid.Roles.Sum(r => r.Users.Where(u => string.IsNullOrWhiteSpace(u.Status)).Count())}/{raid.Roles.Sum(r => r.Spots)}):");
            foreach (ApiRaid.Role role in raid.Roles.OrderBy(x => x.RoleId))
            {
                //print signed up users
                string signedUpUsers = PrintUsers(role);

                if (string.IsNullOrEmpty(signedUpUsers)) signedUpUsers = "-";
                string fieldName = $"{role.Name}: {role.Description} ({role.Users.Where(u => string.IsNullOrWhiteSpace(u.Status)).Count()}/{role.Spots})";

                embed.AddField(fieldName, signedUpUsers);
            }
        }

        private string PrintUsers(ApiRaid.Role role)
        {
            string rolesString = string.Empty;
            foreach (ApiRaid.Role.User user in role.Users.OrderBy(u => u.Status))
            {
                string status = !string.IsNullOrWhiteSpace(user.Status) ? $" - {user.Status}" : string.Empty;
                if(string.IsNullOrEmpty(user.AccountName))
                {
                    rolesString += $"\t{user.UserName} {status}\n";
                }
                else
                {
                    rolesString += $"\t{user.UserName} ({user.AccountName}) {status}\n";
                }
            }
            return rolesString;
        }

        public static ButtonParameters ParseButtonId(string customId)
        {
            ButtonParameters parameters = new ButtonParameters();

            string[] ids = customId.Split('-');
            if(ids.Length > 0)
            {
                parameters.ButtonType = ids[0];
            }
            if(ids.Length > 1)
            {
                int.TryParse(ids[1],out parameters.RaidId);
            }
            return parameters;
        }

        public class ButtonParameters
        {
            public int RaidId;
            public string ButtonType = string.Empty;
        }
    }
}
