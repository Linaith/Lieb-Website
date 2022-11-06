using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Lieb.Data;
using Lieb.Models.GuildWars2.Raid;
using SharedClasses.SharedModels;

namespace Lieb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiscordBotController : ControllerBase
    {
        RaidService _raidService;

        public DiscordBotController(RaidService raidService)
        {
            _raidService = raidService;
        }

        [HttpGet]
        [Route("[action]/{raidId}/{userId}")]
        public List<ApiRole> GetRoles(int raidId, ulong userId)
        {
            Raid raid = _raidService.GetRaid(raidId);
            if(!_raidService.IsRaidSignUpAllowed(userId, raidId, out string errorMessage))
            {
                //TODO: send error message
            }

            List<ApiRole> apiRoles = new List<ApiRole>();
            foreach(RaidRole role in raid.Roles)
            {
                apiRoles.Add(new ApiRole(){
                    Name = role.Name,
                    Description = role.Description,
                    IsSignUpAllowed = _raidService.IsRoleSignUpAllowed(userId, role.RaidRoleId, SignUpType.SignedUp)
                });
            }
            return apiRoles;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignUp(ApiSignUp signUp)
        {
            _raidService.SignUp(signUp.raidId, signUp.userId, signUp.gw2AccountId, signUp.roleId, )
        }
    }
}