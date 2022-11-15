using SharedClasses.SharedModels;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using System.Text;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class DiscordService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private readonly JsonSerializerOptions _serializerOptions;

        public DiscordService(IHttpClientFactory httpClientFactory, IDbContextFactory<LiebContext> contextFactory)
        {
            _httpClientFactory = httpClientFactory;
            _contextFactory = contextFactory;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task PostRaidMessage(int raidId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                Raid raid = context.Raids
                    .Include(r => r.Roles)
                    .Include(r => r.SignUps)
                    .ThenInclude(s => s.LiebUser)
                    .Include(r => r.SignUps)
                    .ThenInclude(s => s.GuildWars2Account)
                    .Include(r => r.SignUps)
                    .ThenInclude(s => s.RaidRole)
                    .Include(r => r.DiscordRaidMessages)
                    .ToList()
                    .FirstOrDefault(r => r.RaidId == raidId, new Raid());

                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientName);

                ApiRaid apiRaid = ConvertRaid(raid);

                var raidItemJson = new StringContent(
                    JsonSerializer.Serialize(apiRaid),
                    Encoding.UTF8,
                    Application.Json);

                string json = JsonSerializer.Serialize(apiRaid);

                var httpResponseMessage = await httpClient.PostAsync("raid/PostRaidMessage", raidItemJson);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream =
                        await httpResponseMessage.Content.ReadAsStreamAsync();
                    
                    ApiRaid returnedRaid = await JsonSerializer.DeserializeAsync<ApiRaid>(contentStream, _serializerOptions);
                    await UpdateDiscordMessages(returnedRaid.DisocrdMessages, raid);
                }
            }
            catch {}
        }

        public async Task DeleteRaidMessages(Raid raid)
        {
            await DeleteRaidMessages(raid.DiscordRaidMessages);
        }

        public async Task DeleteRaidMessages(IEnumerable<DiscordRaidMessage> messages)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientName);

                IEnumerable<ApiRaid.DiscordMessage> apiMessages = ConvertMessages(messages);

                var messageItemJson = new StringContent(
                    JsonSerializer.Serialize(apiMessages),
                    Encoding.UTF8,
                    Application.Json);

                var httpResponseMessage = await httpClient.PostAsync("raid/DeleteRaidMessage", messageItemJson);

                httpResponseMessage.EnsureSuccessStatusCode();
            }
            catch {}
        }

        public async Task<List<DiscordServer>> GetServers()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientName);

                var httpResponseMessage = await httpClient.GetAsync("raid/GetServers");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream =
                        await httpResponseMessage.Content.ReadAsStreamAsync();

                    return await JsonSerializer.DeserializeAsync<List<DiscordServer>>(contentStream, _serializerOptions);
                }
            }
            catch {}
            return new List<DiscordServer>();
        }

        public async Task SendUserReminder(RaidReminder reminder, Raid raid)
        {
            if (await SendMessageToRaidUsers(reminder.Message, raid))
            {
                using var context = _contextFactory.CreateDbContext();
                reminder.Sent = true;
                context.Update(reminder);
                await context.SaveChangesAsync();
            }
        }


        public async Task<bool> SendMessageToRaidUsers(string message, Raid raid)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientName);

                if(raid == null) return false;

                ApiUserReminder apiReminder = ConvertUserReminder(message, raid);

                var raidItemJson = new StringContent(
                    JsonSerializer.Serialize(apiReminder),
                    Encoding.UTF8,
                    Application.Json);

                var httpResponseMessage = await httpClient.PostAsync("raid/SendUserReminder", raidItemJson);

                return httpResponseMessage.IsSuccessStatusCode;
            }
            catch {}
            return false;
        }

        public async Task SendChannelReminder(RaidReminder reminder)
        {
            if (await SendChannelMessage(reminder.DiscordServerId, reminder.DiscordChannelId, reminder.Message))
            {                
                reminder.Sent = true;
                using var context = _contextFactory.CreateDbContext();
                context.Update(reminder);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> SendChannelMessage(ulong discordServerId, ulong discordChannelId, string message)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientName);

                ApiChannelReminder apiReminder = ConvertChannelReminder(discordServerId, discordChannelId, message);

                var raidItemJson = new StringContent(
                    JsonSerializer.Serialize(apiReminder),
                    Encoding.UTF8,
                    Application.Json);

                var httpResponseMessage = await httpClient.PostAsync("raid/SendChannelReminder", raidItemJson);

                return httpResponseMessage.IsSuccessStatusCode;
            }
            catch {}
            return false;
        }

        private async Task UpdateDiscordMessages(IEnumerable<ApiRaid.DiscordMessage> messages, Raid raid)
        {
            foreach(ApiRaid.DiscordMessage message in messages)
            {
                if(raid.DiscordRaidMessages.Where(m => m.DiscordRaidMessageId == message.WebsiteDatabaseId).Any())
                {
                    raid.DiscordRaidMessages.FirstOrDefault(m => m.DiscordRaidMessageId == message.WebsiteDatabaseId).DiscordMessageId = message.MessageId;
                }
            }

            using var context = _contextFactory.CreateDbContext();
            foreach(DiscordRaidMessage message in raid.DiscordRaidMessages)
            {
                context.Update(message);
            }
            await context.SaveChangesAsync();
        }

        public static ApiRaid ConvertRaid(Raid raid)
        {
            ApiRaid apiRaid = new ApiRaid(){
                Title = raid.Title,
                Description = raid.Description,
                Guild = raid.Guild,
                Organizer = raid.Organizer,
                VoiceChat = raid.VoiceChat,
                StartTimeUTC = raid.StartTimeUTC,
                EndTimeUTC = raid.EndTimeUTC,
                RaidId = raid.RaidId
            };
            apiRaid.DisocrdMessages = ConvertMessages(raid.DiscordRaidMessages);
            apiRaid.Roles = new List<ApiRaid.Role>();
            foreach(RaidRole role in raid.Roles)
            {
                ApiRaid.Role apiRole = new ApiRaid.Role(){
                    RoleId = role.RaidRoleId,
                    Description = role.Description,
                    Name = role.Name,
                    Spots = role.Spots
                };
                apiRole.Users = new List<ApiRaid.Role.User>();

                foreach(RaidSignUp signUp in raid.SignUps.Where(x => x.RaidRoleId == role.RaidRoleId))
                {
                    if(signUp.SignUpType != SignUpType.SignedOff)
                    {
                        if(signUp.IsExternalUser)
                        {
                            string status = signUp.SignUpType != SignUpType.SignedUp ? signUp.SignUpType.ToString() : string.Empty;
                            apiRole.Users.Add(new ApiRaid.Role.User(){
                                AccountName = string.Empty,
                                Status = status,
                                UserName = signUp.ExternalUserName
                            });
                        }
                        else
                        {
                            string status = signUp.SignUpType != SignUpType.SignedUp ? signUp.SignUpType.ToString() : string.Empty;
                            apiRole.Users.Add(new ApiRaid.Role.User(){
                                AccountName = signUp.GuildWars2Account.AccountName,
                                Status = status,
                                UserName = signUp.LiebUser.Name,
                                UserId = signUp.LiebUserId.Value
                            });
                        }
                    }
                }
                apiRaid.Roles.Add(apiRole);
            }
            return apiRaid;
        }

        public static List<ApiRaid.DiscordMessage> ConvertMessages(IEnumerable<DiscordRaidMessage> messages)
        {
            List<ApiRaid.DiscordMessage> apiMessages = new List<ApiRaid.DiscordMessage>();
            foreach(DiscordRaidMessage message in messages)
            {
                apiMessages.Add(new ApiRaid.DiscordMessage(){
                    ChannelId = message.DiscordChannelId,
                    GuildId = message.DiscordGuildId,
                    MessageId = message.DiscordMessageId,
                    WebsiteDatabaseId = message.DiscordRaidMessageId
                });
            }
            return apiMessages;
        }

        public static ApiUserReminder ConvertUserReminder(string message, Raid raid)
        {
            ApiUserReminder apiReminder = new ApiUserReminder()
            {
                Message = message
            };
            apiReminder.UserIds = new List<ulong>();
            foreach(RaidSignUp signUp in raid.SignUps)
            {
                if(signUp.LiebUserId.HasValue)
                {
                    apiReminder.UserIds.Add(signUp.LiebUserId.Value);
                }
            }
            return apiReminder;
        }

        public static ApiChannelReminder ConvertChannelReminder(ulong discordServerId, ulong discordChannelId, string message)
        {
            return new ApiChannelReminder()
            {
                DiscordServerId = discordServerId,
                DiscordChannelId = discordChannelId,
                Message = message
            };
        }
    }
}