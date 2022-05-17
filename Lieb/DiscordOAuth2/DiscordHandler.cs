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
        private readonly LiebContext _dbContext;
        private readonly UserService _userService;

        public DiscordHandler(IOptionsMonitor<DiscordOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, LiebContext context, UserService userService)
            : base(options, logger, encoder, clock)
        {
            _dbContext = context;
            _userService = userService;
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

            //debug
            //context.Identity.AddClaim(new Claim(Constants.ClaimType, Constants.Roles.User));

            context = await ManageUserRights(context);

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }

        private async Task<OAuthCreatingTicketContext> ManageUserRights(OAuthCreatingTicketContext context)
        {
            ulong discordId = ulong.Parse(context.Identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
            LiebUser? user = await _dbContext.LiebUsers.Include(u => u.RoleAssignments).ThenInclude(r => r.LiebRole).FirstOrDefaultAsync(m => m.Id == discordId);
            if (user != null)
            {
                if (user.BannedUntil == null || user.BannedUntil < DateTime.UtcNow)
                {
                    foreach (RoleAssignment role in user.RoleAssignments)
                    {
                        context.Identity.AddClaim(new Claim(Constants.ClaimType, role.LiebRole.RoleName));
                    }
                }
            }
            else
            {
                string userName = context.Identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value;

                _userService.CreateUser(discordId, userName);

                context.Identity.AddClaim(new Claim(Constants.ClaimType, Constants.Roles.User.Name));
            }
            return context;
        }
    }
}
