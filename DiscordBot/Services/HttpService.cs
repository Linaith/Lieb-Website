using SharedClasses.SharedModels;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using System.Text;

namespace DiscordBot.Services
{
    public class HttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _serializerOptions;

        public HttpService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<ApiRole>> GetRoles(int raidId, ulong userId)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);

            var httpResponseMessage = await httpClient.GetAsync($"DiscordBot/GetRoles/{raidId}/{userId}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream =
                    await httpResponseMessage.Content.ReadAsStreamAsync();

                return await JsonSerializer.DeserializeAsync<List<ApiRole>>(contentStream, _serializerOptions);
            }
            return new List<ApiRole>();
        }

        public async Task SignUp(ApiSignUp signUp)
        {
            await SendSignUp(signUp, "DiscordBot/SignUp");
        }

        public async Task SignUpMaybe(ApiSignUp signUp)
        {
            await SendSignUp(signUp, "DiscordBot/SignUpMaybe");
        }

        public async Task SignUpBackup(ApiSignUp signUp)
        {
            await SendSignUp(signUp, "DiscordBot/SignUpBackup");
        }

        public async Task SignUpFlex(ApiSignUp signUp)
        {
            await SendSignUp(signUp, "DiscordBot/SignUpFlex");
        }

        public async Task SignOff(ApiSignUp signUp)
        {
            await SendSignUp(signUp, "DiscordBot/SignOff");
        }

        private async Task SendSignUp(ApiSignUp signUp, string requestUri)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);

            var raidItemJson = new StringContent(
                JsonSerializer.Serialize(signUp),
                Encoding.UTF8,
                Application.Json);

            var httpResponseMessage = await httpClient.PostAsync(requestUri, raidItemJson);
        }

    }
}