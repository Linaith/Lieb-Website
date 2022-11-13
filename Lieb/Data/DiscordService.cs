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

        public async Task SendUserReminder(RaidReminder reminder)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientName);

                Raid raid = context.Raids
                    .Include(r => r.SignUps)
                    .ThenInclude(s => s.LiebUser)
                    .FirstOrDefault(r => r.RaidId == reminder.RaidId);

                if(raid == null) return;

                ApiUserReminder apiReminder = ConvertUserReminder(reminder, raid);

                var raidItemJson = new StringContent(
                    JsonSerializer.Serialize(apiReminder),
                    Encoding.UTF8,
                    Application.Json);

                var httpResponseMessage = await httpClient.PostAsync("raid/SendUserReminder", raidItemJson);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    reminder.Sent = true;
                    context.Update(reminder);
                    await context.SaveChangesAsync();
                }
            }
            catch {}
        }

        public async Task SendChannelReminder(RaidReminder reminder)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientName);

                ApiChannelReminder apiReminder = ConvertChannelReminder(reminder);

                var raidItemJson = new StringContent(
                    JsonSerializer.Serialize(apiReminder),
                    Encoding.UTF8,
                    Application.Json);

                var httpResponseMessage = await httpClient.PostAsync("raid/SendChannelReminder", raidItemJson);

                if (httpResponseMessage.IsSuccessStatusCode)
                {                
                    reminder.Sent = true;
                    using var context = _contextFactory.CreateDbContext();
                    context.Update(reminder);
                    await context.SaveChangesAsync();
                }
            }
            catch {}
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

        private ApiRaid ConvertRaid(Raid raid)
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
                                UserName = signUp.LiebUser.Name
                            });
                        }
                    }
                }
                apiRaid.Roles.Add(apiRole);
            }
            return apiRaid;
        }

        private List<ApiRaid.DiscordMessage> ConvertMessages(IEnumerable<DiscordRaidMessage> messages)
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

        private ApiUserReminder ConvertUserReminder(RaidReminder reminder, Raid raid)
        {
            ApiUserReminder apiReminder = new ApiUserReminder()
            {
                Message = reminder.Message
            };
            apiReminder.UserIds = new List<ulong>();
            foreach(RaidSignUp signUp in raid.SignUps)
            {
                if(signUp.LiebUserId > 0)
                {
                    apiReminder.UserIds.Add(signUp.LiebUserId);
                }
            }
            return apiReminder;
        }

        private ApiChannelReminder ConvertChannelReminder(RaidReminder reminder)
        {
            return new ApiChannelReminder()
            {
                DiscordServerId = reminder.DiscordServerId,
                DiscordChannelId = reminder.DiscordChannelId,
                Message = reminder.Message
            };
        }
    }
}