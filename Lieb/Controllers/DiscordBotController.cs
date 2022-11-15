using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Lieb.Data;
using Lieb.Models.GuildWars2.Raid;
using Lieb.Models.GuildWars2;
using Lieb.Models;
using SharedClasses.SharedModels;

namespace Lieb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiscordBotController : ControllerBase
    {
        RaidService _raidService;
        UserService _userService;
        GuildWars2AccountService _gw2AccountService;

        public DiscordBotController(RaidService raidService, UserService userService, GuildWars2AccountService gw2AccountService)
        {
            _raidService = raidService;
            _userService = userService;
            _gw2AccountService = gw2AccountService;
        }

        [HttpGet]
        [Route("[action]/{userId}")]
        public ActionResult<bool> DoesUserExist(ulong userId)
        {
            LiebUser user = _userService.GetLiebUserGW2AccountOnly(userId);
            return user != null && user.GuildWars2Accounts.Count() > 0;
        }

        [HttpGet]
        [Route("[action]/{raidId}/{userId}")]
        public ActionResult IsSignUpAllowed(int raidId, ulong userId)
        {
            if(!_raidService.IsRaidSignUpAllowed(userId, raidId, out string errorMessage))
            {
                return Problem(errorMessage);
            }
            return Ok();
        }

        [HttpGet]
        [Route("[action]/{raidId}/{userId}")]
        public ActionResult<List<ApiRole>> GetRoles(int raidId, ulong userId)
        {
            Raid raid = _raidService.GetRaid(raidId);

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
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
                await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.SignedUp, signUp.signedUpByUserId);
            }
            else
            {
                await _raidService.SignUpExternalUser(signUp.raidId, signUp.userName, signUp.roleId, SignUpType.SignedUp, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignUpMaybe(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
                await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.Maybe, signUp.signedUpByUserId);
            }
            else
            {
                await _raidService.SignUpExternalUser(signUp.raidId, signUp.userName, signUp.roleId, SignUpType.Maybe, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignUpBackup(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
                await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.Backup, signUp.signedUpByUserId);
            }
            else
            {
                await _raidService.SignUpExternalUser(signUp.raidId, signUp.userName, signUp.roleId, SignUpType.Backup, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignUpFlex(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
                await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.Flex, signUp.signedUpByUserId);
            }
            else
            {
                await _raidService.SignUpExternalUser(signUp.raidId, signUp.userName, signUp.roleId, SignUpType.Flex, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task SignOff(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetLiebUserGW2AccountOnly(signUp.userId).GuildWars2Accounts.FirstOrDefault(new GuildWars2Account()).GuildWars2AccountId;
                await _raidService.SignOff(signUp.raidId, signUp.userId, signUp.signedUpByUserId);
            }
            else
            {
                await _raidService.SignOffExternalUser(signUp.raidId, signUp.userName, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> CreateAccount(ApiRaid.Role.User user)
        {
            if(!Regex.IsMatch(user.AccountName, Constants.GW2_ACCOUNT_REGEX))
            {
                return Problem("Invalid Account Name");
            }

            GuildWars2Account gw2Account = new GuildWars2Account()
            {
                AccountName = user.AccountName
            };
            await _userService.CreateUser(user.UserId, user.UserName);
            await _gw2AccountService.AddOrEditAccount(gw2Account, user.UserId);
            return Ok();
        }

        [HttpGet]
        [Route("[action]/{raidId}")]
        public ActionResult<ApiRaid> GetRaid(int raidId)
        {
            Raid raid = _raidService.GetRaid(raidId);

            return DiscordService.ConvertRaid(raid);
        }
    }
}