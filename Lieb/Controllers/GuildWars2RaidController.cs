using Lieb.Data;
using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Lieb.Controllers
{
    public class GuildWars2RaidController : ControllerBase
    {
        private readonly RaidService _raidService;
        private readonly UserService _userService;
        private readonly IDbContextFactory<LiebContext> _contextFactory;

        public GuildWars2RaidController(RaidService raidService, UserService userService, IDbContextFactory<LiebContext> contextFactory)
        {
            _raidService = raidService;
            _userService = userService;
            _contextFactory = contextFactory;
        }
        

        [HttpGet]
        public int GetRaidIdByDiscordMessageId(int discordMessageId)
        {
            using var context = _contextFactory.CreateDbContext();
            DiscordRaidMessage discordMessage = context.DiscordRaidMessages.FirstOrDefault(m => m.DiscordRaidMessageId == discordMessageId);
            //TODO retrun 404
            return discordMessage != null ? discordMessage.RaidId : -1;
        }

        [HttpGet]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRaidIdByDiscordMessageId([FromRoute] ulong discordMessageId)
        {
            using var context = _contextFactory.CreateDbContext();
            DiscordRaidMessage? discordMessage = await context.DiscordRaidMessages.FirstOrDefaultAsync(m => m.DiscordMessageId == discordMessageId);
            if (discordMessage == null)
            {
                return Problem(statusCode: (int)HttpStatusCode.NotFound);
            }
            return Ok(discordMessage.RaidId);
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<RaidRole>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetFreeRoles([FromRoute] int raidId)
        {
            Raid raid = _raidService.GetRaid(raidId);
            Dictionary<int, int> roleMap = new Dictionary<int, int>();
            foreach(RaidSignUp signUp in raid.SignUps)
            {
                if (signUp.SignUpType == SignUpType.SignedUp)
                {
                    roleMap[signUp.RaidRoleId] += 1;
                }
            }
            return Ok(raid.Roles.Where(r => roleMap.ContainsKey(r.RaidRoleId) && roleMap[r.RaidRoleId] < r.Spots).ToList());
        }



        [HttpGet]
        [ProducesResponseType(typeof(List<GuildWars2Account>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetGuildwars2Accounts([FromRoute] ulong userId)
        {
            using var context = _contextFactory.CreateDbContext();
            List<GuildWars2Account>? gw2Accounts = (await context.LiebUsers.FirstOrDefaultAsync(u => u.Id == userId))?.GuildWars2Accounts.ToList();
            if (gw2Accounts == null)
            {
                return Problem(statusCode: (int)HttpStatusCode.NotFound);
            }
            return Ok(gw2Accounts);
        }

        [HttpPost]
        public string SignUp(int raidId, ulong discordUserId, int guildWars2AccountId, int roleId, SignUpType signUpType)
        {
            int userId = _userService.GetLiebUserId(discordUserId).Result;
            _raidService.SignUp(raidId, userId, guildWars2AccountId, roleId, signUpType).Wait();
            return String.Empty;
        }


        [HttpGet("metadata/{fileId}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetFileMetadata([FromRoute] Guid fileId)
        {
            using var context = _contextFactory.CreateDbContext();
            DiscordRaidMessage discordMessage = context.DiscordRaidMessages.FirstOrDefault(m => m.DiscordRaidMessageId == discordMessageId);
            if (discordMessage == null)
            {
                return Problem(statusCode: (int)HttpStatusCode.NotFound);
            }
            return Ok(discordMessage.RaidId);
        }







        [HttpGet("metadata/{fileId}")]
        [Authorize(Roles = Roles.TimeBoard)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetFileMetadata([FromRoute] Guid fileId)
        {
            using var context = _contextFactory.CreateDbContext();
            DiscordRaidMessage discordMessage = context.DiscordRaidMessages.FirstOrDefault(m => m.DiscordRaidMessageId == discordMessageId);
            if(discordMessage == null)
            {
                return Problem(statusCode: (int)HttpStatusCode.NotFound);
            }
            return Ok(discordMessage.RaidId);
        }
        
    }
}
