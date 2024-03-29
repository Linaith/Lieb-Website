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
        PollService _pollService;
        GuildWars2AccountService _gw2AccountService;
        DiscordService _discordService;

        public DiscordBotController(RaidService raidService, UserService userService, GuildWars2AccountService gw2AccountService, DiscordService discordService, PollService pollService)
        {
            _raidService = raidService;
            _userService = userService;
            _gw2AccountService = gw2AccountService;
            _discordService = discordService;
            _pollService = pollService;
        }

        [HttpGet]
        [Route("[action]/{userId}")]
        public ActionResult<bool> DoesUserExist(ulong userId)
        {
            LiebUser user = _userService.GetLiebUserGW2AccountOnly(userId);
            return user != null && user.GuildWars2Accounts.Count() > 0;
        }

        [HttpGet]
        [Route("[action]/{raidId}/{userId}/{ignoreRole}")]
        public ActionResult IsSignUpAllowed(int raidId, ulong userId, bool ignoreRole)
        {
            if(!_raidService.IsRaidSignUpAllowed(userId, raidId, out string errorMessage, ignoreRole, true))
            {
                return Problem(errorMessage);
            }
            return Ok();
        }

        [HttpGet]
        [Route("[action]/{raidId}/{userId}")]
        public string IsUserSignedUp(int raidId, ulong userId)
        {
            Raid raid = _raidService.GetRaid(raidId);
            RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userId && s.SignUpType != SignUpType.Flex);
            if(signUp == null) return string.Empty;

            switch(signUp.SignUpType)
            {
                case SignUpType.SignedUp:
                    return SharedConstants.SIGNED_UP;
                case SignUpType.Maybe:
                    return SharedConstants.MAYBE;
                case SignUpType.Backup:
                    return SharedConstants.BACKUP;
            }
            return string.Empty;
        }

        [HttpGet]
        [Route("[action]/{raidId}")]
        public ActionResult IsExternalSignUpAllowed(int raidId)
        {
            if(!_raidService.IsExternalSignUpAllowed(raidId, out string errorMessage))
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
            if(raid.RaidType == RaidType.Planned)
            {
                foreach(RaidRole role in raid.Roles)
                {
                    apiRoles.Add(new ApiRole(){
                        Name = role.Name,
                        Description = role.Description,
                        IsSignUpAllowed = _raidService.IsRoleSignUpAllowed(raidId, userId, role.RaidRoleId, SignUpType.SignedUp, false),
                        roleId = role.RaidRoleId
                    });
                }
            }
            else
            {
                RaidRole role = raid.Roles.First(r => r.IsRandomSignUpRole);
                apiRoles.Add(new ApiRole(){
                        Name = role.Name,
                        Description = role.Description,
                        IsSignUpAllowed = _raidService.IsRoleSignUpAllowed(raidId, userId, role.RaidRoleId, SignUpType.SignedUp, false),
                        roleId = role.RaidRoleId
                    });
            }
            return apiRoles;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> SignUp(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetSignUpAccount(signUp.userId, signUp.raidId, signUp.gw2AccountId);
                return await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.SignedUp, signUp.signedUpByUserId);
            }
            else
            {
                return await _raidService.SignUpExternalUser(signUp.raidId, signUp.userName, signUp.roleId, SignUpType.SignedUp, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> SignUpMaybe(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetSignUpAccount(signUp.userId, signUp.raidId, signUp.gw2AccountId);
                return await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.Maybe, signUp.signedUpByUserId);
            }
            else
            {
                return await _raidService.SignUpExternalUser(signUp.raidId, signUp.userName, signUp.roleId, SignUpType.Maybe, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> SignUpBackup(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetSignUpAccount(signUp.userId, signUp.raidId, signUp.gw2AccountId);
                return await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.Backup, signUp.signedUpByUserId);
            }
            else
            {
                return await _raidService.SignUpExternalUser(signUp.raidId, signUp.userName, signUp.roleId, SignUpType.Backup, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> SignUpFlex(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                int accountId = _userService.GetSignUpAccount(signUp.userId, signUp.raidId, signUp.gw2AccountId);
                return await _raidService.SignUp(signUp.raidId, signUp.userId, accountId, signUp.roleId, SignUpType.Flex, signUp.signedUpByUserId);
            }
            else
            {
                return await _raidService.SignUpExternalUser(signUp.raidId, signUp.userName, signUp.roleId, SignUpType.Flex, signUp.signedUpByUserId);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> SignOff(ApiSignUp signUp)
        {
            if(signUp.userId != 0)
            {
                await _raidService.SignOff(signUp.raidId, signUp.userId, signUp.signedUpByUserId);
            }
            else
            {
                await _raidService.SignOffExternalUser(signUp.raidId, signUp.userName, signUp.signedUpByUserId);
            }
            return true;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> ChangeSignUpTypeToSignUp(ApiSignUp signUp)
        {
            await _raidService.ChangeSignUpType(signUp.raidId, signUp.userId, SignUpType.SignedUp,  true);
            return true;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> ChangeSignUpTypeToMaybe(ApiSignUp signUp)
        {
            await _raidService.ChangeSignUpType(signUp.raidId, signUp.userId, SignUpType.Maybe,  true);
            return true;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> ChangeSignUpTypeToBackup(ApiSignUp signUp)
        {
            await _raidService.ChangeSignUpType(signUp.raidId, signUp.userId, SignUpType.Backup,  true);
            return true;
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
            if(_userService.GetLiebUserGW2AccountOnly(user.UserId) == null)
            {
                await _userService.CreateUser(user.UserId, user.UserName);
            }
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

        [HttpGet]
        [Route("[action]")]
        public List<ulong> GetUserRenameServers()
        {
            return _discordService.GetUserRenameServers();
        }

        [HttpGet]
        [Route("[action]/{userId}")]
        public ActionResult<ApiRaid.Role.User> GetUser(ulong userId)
        {
            LiebUser user = _userService.GetLiebUser(userId);

            if(user != null)
            {
                return Ok(new ApiRaid.Role.User(){
                    UserId = user.Id,
                    UserName = user.Name,
                    AccountName = user.GuildWars2Accounts.FirstOrDefault(a => a.GuildWars2AccountId == user.MainGW2Account, new GuildWars2Account()).AccountName
                });
            }

            return Problem("user not found");
        }

        [HttpGet]
        [Route("[action]/{userId}/{raidId}")]
        public ActionResult<List<ApiGuildWars2Account>> GetSignUpAccounts(ulong userId, int raidId)
        {
            List<GuildWars2Account> accounts = _userService.GetDiscordSignUpAccounts(userId, raidId);
            List<ApiGuildWars2Account> apiAccounts = new List<ApiGuildWars2Account>();

            foreach(GuildWars2Account account in accounts)
            {
                apiAccounts.Add(new ApiGuildWars2Account(){
                    AccountName = account.AccountName,
                    GuildWars2AccountId = account.GuildWars2AccountId
                });
            }
            return Ok(apiAccounts);
        }

        [HttpGet]
        [Route("[action]/{userId}/{command}")]
        public ActionResult IsSlashCommandAllowed(ulong userId, string command)
        {
            switch(command)
            {
                case SharedConstants.SlashCommands.RAID:
                    if(!_raidService.IsRaidSlashCommandAllowed(userId, out string errorMessage))
                    {
                        return Problem(errorMessage);
                    }
                break;
                default:
                    return Problem("command not found on server");
                break;
            }
            return Ok();
        }

        [HttpGet]
        [Route("[action]/{userId}")]
        public async Task<ActionResult> ReminderOptOut(ulong userId)
        {
            return Ok(await _userService.ReminderOptOut(userId));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task AnswerPoll(ApiPollAnswer answer)
        {
            await _pollService.UpdateAnswer(answer.PollId, answer.OptionId, answer.Answer, answer.UserId);
        }
    }
}