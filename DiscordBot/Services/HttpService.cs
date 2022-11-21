using SharedClasses.SharedModels;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<bool> DoesUserExist(ulong userId)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);

            var httpResponseMessage = await httpClient.GetAsync($"DiscordBot/DoesUserExist/{userId}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream =
                    await httpResponseMessage.Content.ReadAsStreamAsync();

                return await JsonSerializer.DeserializeAsync<bool>(contentStream, _serializerOptions);
            }
            return false;
        }

        public async Task<Tuple<bool, string>> IsSignUpAllowed(int raidId, ulong userId, bool ignoreRole = false)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);

            var httpResponseMessage = await httpClient.GetAsync($"DiscordBot/IsSignUpAllowed/{raidId}/{userId}/{ignoreRole}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                ProblemDetails problemDetails = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(_serializerOptions) ?? new ProblemDetails();
                string errorMessage = string.IsNullOrEmpty(problemDetails.Detail) ? string.Empty : problemDetails.Detail; 
                return new Tuple<bool, string>(false, errorMessage);
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public async Task<Tuple<bool, string>> IsExternalSignUpAllowed(int raidId)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);

            var httpResponseMessage = await httpClient.GetAsync($"DiscordBot/IsExternalSignUpAllowed/{raidId}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                ProblemDetails problemDetails = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(_serializerOptions) ?? new ProblemDetails();
                string errorMessage = string.IsNullOrEmpty(problemDetails.Detail) ? string.Empty : problemDetails.Detail; 
                return new Tuple<bool, string>(false, errorMessage);
            }
            return new Tuple<bool, string>(true, string.Empty);
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

        public async Task<Tuple<bool, string>> CreateAccount(ApiRaid.Role.User user)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);

            var raidItemJson = new StringContent(
                JsonSerializer.Serialize(user),
                Encoding.UTF8,
                Application.Json);

            var httpResponseMessage = await httpClient.PostAsync("DiscordBot/CreateAccount", raidItemJson);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                ProblemDetails problemDetails = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(_serializerOptions) ?? new ProblemDetails();
                string errorMessage = string.IsNullOrEmpty(problemDetails.Detail) ? string.Empty : problemDetails.Detail; 
                return new Tuple<bool, string>(false, errorMessage);
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public async Task<ApiRaid> GetRaid(int raidId)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);

            var httpResponseMessage = await httpClient.GetAsync($"DiscordBot/GetRaid/{raidId}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream =
                    await httpResponseMessage.Content.ReadAsStreamAsync();

                return await JsonSerializer.DeserializeAsync<ApiRaid>(contentStream, _serializerOptions);
            }
            return new ApiRaid();
        }

        public async Task<List<ulong>> GetUserRenameServers()
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);

            var httpResponseMessage = await httpClient.GetAsync($"DiscordBot/GetUserRenameServers");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream =
                    await httpResponseMessage.Content.ReadAsStreamAsync();

                return await JsonSerializer.DeserializeAsync<List<ulong>>(contentStream, _serializerOptions);
            }
            return new List<ulong>();
        }
    }
}