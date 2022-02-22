using Lieb.Data;
using Lieb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Discord.OAuth2
{
    internal class DiscordHandler : OAuthHandler<DiscordOptions>
    {
        private readonly Lieb.Data.LiebContext _LiebDbcontext;

        public DiscordHandler(IOptionsMonitor<DiscordOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, Lieb.Data.LiebContext context)
            : base(options, logger, encoder, clock)
        {
            _LiebDbcontext = context;
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await Backchannel.SendAsync(request, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to retrieve Discord user information ({response.StatusCode}).");

            var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
            context.RunClaimActions();

            await Events.CreatingTicket(context);

            LiebUser? user = await _LiebDbcontext.LiebUsers.Include(u => u.Roles).FirstOrDefaultAsync(m => m.DiscordUserId == 1);
            if (user != null)
            {
                foreach (UserRole role in user.Roles)
                {
                    context.Identity.AddClaim(new Claim(Constants.ClaimType, role.RoleName));
                }
            }

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }
    }
}
