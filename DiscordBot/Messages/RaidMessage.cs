using Discord;
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
            var menuBuilder = new SelectMenuBuilder()
               .WithPlaceholder("Select an option")
               .WithCustomId("menu-1")
               .WithMinValues(1)
               .WithMaxValues(1)
               .AddOption("Option A", "opt-a", "Option B is lying!")
               .AddOption("Option B", "opt-b", "Option A is telling the truth!");

            var builder = new ComponentBuilder()
                .WithButton("SignUp", $"{Constants.ComponentIds.SIGN_UP}-{raid.RaidId.ToString()}", ButtonStyle.Secondary)
                .WithButton("SignOff", $"{Constants.ComponentIds.SIGN_OFF}-{raid.RaidId.ToString()}", ButtonStyle.Secondary);
                //.WithSelectMenu(menuBuilder);

            MessageComponent components = builder.Build();
            Embed raidMessage = CreateRaidMessage(raid);

            foreach (ApiRaid.DiscordMessage message in raid.DisocrdMessages)
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
                        await messageChannel.ModifyMessageAsync(message.MessageId, new Action<MessageProperties>(x => x = properties));
                    }
                    else
                    {
                        IUserMessage sentMessage = await messageChannel.SendMessageAsync(embed: raidMessage, components: components);
                        message.MessageId = sentMessage.Id;
                    }
                }
            }
            return raid;
        }

        public Embed CreateRaidMessage(ApiRaid raid)
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
            embed.AddField("Date", $"{raid.StartTimeUTC.ToLocalTime().DateTime.ToLongDateString()}");
            embed.AddField("Time", $"from: {raid.StartTimeUTC.ToLocalTime().DateTime.ToShortTimeString()}  to: {raid.EndTimeUTC.ToLocalTime().DateTime.ToShortTimeString()}");
            embed.AddField("Organisator", raid.Organizer, true);
            embed.AddField("Guild", raid.Guild, true);
            embed.AddField("Voice chat", raid.VoiceChat, true);
        }

        private void AddMessageRoles(ApiRaid raid, ref EmbedBuilder embed)
        {
            Dictionary<string, string> fieldList = new Dictionary<string, string>();

            embed.AddField("Signed up", $"({raid.Roles.Sum(r => r.Users.Count)}/{raid.Roles.Sum(r => r.Spots)}):");
            foreach (ApiRaid.Role role in raid.Roles)
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
                rolesString += $"\t{user.UserName} ({user.AccountName}) {status}\n";
            }
            return rolesString;
        }
    }
}
