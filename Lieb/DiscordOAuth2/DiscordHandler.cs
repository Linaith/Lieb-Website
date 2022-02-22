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

            //debug
            //context.Identity.AddClaim(new Claim(Constants.ClaimType, Constants.Roles.User));

            context = await ManageUserRights(context);

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }

        private async Task<OAuthCreatingTicketContext> ManageUserRights(OAuthCreatingTicketContext context)
        {
            ulong discordId = ulong.Parse(context.Identity.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value);
            LiebUser? user = await _LiebDbcontext.LiebUsers.Include(u => u.RoleAssignments).ThenInclude(r => r.LiebRole).FirstOrDefaultAsync(m => m.DiscordUserId == discordId);
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
                LiebRole standardRole = await _LiebDbcontext.LiebRoles.FirstOrDefaultAsync(m => m.RoleName == Constants.Roles.User);
                LiebUser newUser = new LiebUser();
                newUser.DiscordUserId = discordId;
                _LiebDbcontext.LiebUsers.Add(newUser);
                await _LiebDbcontext.SaveChangesAsync();
                RoleAssignment roleAssignment = new RoleAssignment()
                {
                    LiebRoleId = standardRole.LiebRoleId,
                    LiebUserId = newUser.LiebUserId
                };
                _LiebDbcontext.RoleAssignments.Add(roleAssignment);
                await _LiebDbcontext.SaveChangesAsync();

                context.Identity.AddClaim(new Claim(Constants.ClaimType, Constants.Roles.User));
            }
            return context;
        }
    }
}
