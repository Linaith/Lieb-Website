using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Lieb.Data;
using Lieb.Models.GuildWars2.Raid;
using Lieb.Models.GuildWars2;
using SharedClasses.SharedModels;

namespace Lieb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiscordBotController : ControllerBase
    {
        RaidService _raidService;
        UserService _userService;

        public DiscordBotController(RaidService raidService, UserService userService)
        {
            _raidService = raidService;
            _userService = userService;
        }

        [HttpGet]
        [Route("[action]/{raidId}/{userId}")]
        public ActionResult<List<ApiRole>> GetRoles(int raidId, ulong userId)
        {
            Raid raid = _raidService.GetRaid(raidId);
            if(!_raidService.IsRaidSignUpAllowed(userId, raidId, out string errorMessage))
            {
                return Problem(errorMessage);
            }

            List<ApiRole> apiRoles = new List<ApiRole>();
            foreach(RaidRole role in raid.Roles)
            {
                apiRoles.Add(new ApiRole(){
                    Name = role.Name,
                    Description = role.Description,
                    IsSignUpAllowed = _raidService.IsRoleSignUpAllowed(userId, role.RaidRoleId, SignUpType.SignedUp),
                    roleId = role.RaidRoleId
                });
            }
            return apiRoles;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignUp(ApiSignUp signUp)
        {
            int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
            await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.SignedUp);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignUpMaybe(ApiSignUp signUp)
        {
            int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
            await _raidService.SignUp(signUp.raidId, signUp.userId, signUp.gw2AccountId, signUp.roleId, SignUpType.Maybe);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignUpBackup(ApiSignUp signUp)
        {
            int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
            await _raidService.SignUp(signUp.raidId, signUp.userId, signUp.gw2AccountId, signUp.roleId, SignUpType.Backup);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignUpFlex(ApiSignUp signUp)
        {
            int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
            await _raidService.SignUp(signUp.raidId, signUp.userId, signUp.gw2AccountId, signUp.roleId, SignUpType.Flex);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignOff(ApiSignUp signUp)
        {
            int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
            await _raidService.SignOff(signUp.raidId, signUp.userId);
        }
    }
}