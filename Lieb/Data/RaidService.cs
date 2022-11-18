using Lieb.Models;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class RaidService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private readonly DiscordService _discordService;

        public RaidService(IDbContextFactory<LiebContext> contextFactory, DiscordService discordService)
        {
            _contextFactory = contextFactory;
            _discordService = discordService;
        }

        public List<Raid> GetRaids()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Raids
                .Include(r => r.Roles)
                .Include(r => r.RaidLogs)
                .Include(r => r.Reminders)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.LiebUser)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.GuildWars2Account)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.RaidRole)
                .Include(r => r.DiscordRaidMessages)
                .ToList();
        }

        public Raid GetRaid(int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Raids
                .Include(r => r.Roles)
                .Include(r => r.RaidLogs)
                .Include(r => r.Reminders)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.LiebUser)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.GuildWars2Account)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.RaidRole)
                .Include(r => r.DiscordRaidMessages)
                .ToList()
                .FirstOrDefault(r => r.RaidId == raidId, new Raid());
        }

        public async Task AddOrEditRaid(Raid raid, List<RaidRole> rolesToDelete, List<RaidReminder> remindersToDelete, List<DiscordRaidMessage> messagesToDelete)
        {
            if (raid != null)
            {
                using var context = _contextFactory.CreateDbContext();
                if (raid.RaidId == 0)
                {
                    context.Raids.Add(raid);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Update(raid);
                    context.RaidRoles.RemoveRange(rolesToDelete);
                    context.RaidReminders.RemoveRange(remindersToDelete);
                    context.DiscordRaidMessages.RemoveRange(messagesToDelete);

                    //move users back to "Random" role
                    if (raid.RaidType != RaidType.Planned)
                    {
                        RaidRole randomRole = raid.Roles.FirstOrDefault(r => r.IsRandomSignUpRole, CreateRandomSignUpRole(raid.RaidType));
                        foreach (RaidSignUp signUp in raid.SignUps)
                        {
                            signUp.RaidRole = randomRole;
                        }
                        context.RaidRoles.RemoveRange(raid.Roles.Where(r => !r.IsRandomSignUpRole));
                    }

                    await context.SaveChangesAsync();
                }
                await _discordService.PostRaidMessage(raid.RaidId);
            }
        }

        public async Task DeleteRaid(int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid raid = GetRaid(raidId);
            context.RaidSignUps.RemoveRange(raid.SignUps);
            context.RaidRoles.RemoveRange(raid.Roles);
            context.RaidLogs.RemoveRange(raid.RaidLogs);
            context.RaidReminders.RemoveRange(raid.Reminders);
            context.DiscordRaidMessages.RemoveRange(raid.DiscordRaidMessages);
            await context.SaveChangesAsync();
            context.Raids.Remove(raid);
            await context.SaveChangesAsync();

            await _discordService.DeleteRaidMessages(raid);
            if(raid.EndTimeUTC > DateTimeOffset.UtcNow)
            {
                await _discordService.SendMessageToRaidUsers($"Raid \"{raid.Title}\": was deleted.", raid);
            }
        }

        public async Task SignUp(int raidId, ulong liebUserId, int guildWars2AccountId, int plannedRoleId, SignUpType signUpType, ulong signedUpByUserId = 0)
        {
            if (!IsRoleSignUpAllowed(raidId, liebUserId, plannedRoleId, signUpType, true))
            {
                return;
            }
            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(r => r.RaidId == raidId && r.LiebUserId == liebUserId).ToList();
            if (signUpType != SignUpType.Flex && signUps.Where(r => r.SignUpType != SignUpType.Flex).Any())
            {
                await ChangeSignUpType(raidId, liebUserId, plannedRoleId, signUpType);
            }
            else if (!signUps.Where(r => r.RaidRoleId == plannedRoleId).Any())
            {
                RaidSignUp signUp = new RaidSignUp(raidId, liebUserId, guildWars2AccountId, plannedRoleId, signUpType);
                string userName = context.LiebUsers.FirstOrDefault(l => l.Id == liebUserId)?.Name;
                context.RaidSignUps.Add(signUp);
                await context.SaveChangesAsync();
                await LogSignUp(signUp, userName, signedUpByUserId);
            }
            await _discordService.PostRaidMessage(raidId);
        }

        public async Task SignUpExternalUser(int raidId, string userName, int plannedRoleId, SignUpType signUpType, ulong signedUpByUserId)
        {
            if (!IsRoleSignUpAllowed(raidId, ulong.MaxValue, plannedRoleId, signUpType, true))
            {
                return;
            }
            using var context = _contextFactory.CreateDbContext();

            
            RaidSignUp signUp = new RaidSignUp(raidId, userName, plannedRoleId, signUpType);
            context.RaidSignUps.Add(signUp);
            await context.SaveChangesAsync();
            await LogSignUp(signUp, userName, signedUpByUserId);
            await _discordService.PostRaidMessage(raidId);
        }

        public async Task SignOff(int raidId, ulong liebUserId, ulong signedOffByUserId = 0)
        {
            using var context = _contextFactory.CreateDbContext();
            //remove Flex Sign Ups
            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId && x.SignUpType == SignUpType.Flex).ToList();
            context.RaidSignUps.RemoveRange(signUps);

            //change to SignedOff
            RaidSignUp? signUp = context.RaidSignUps.Include(s => s.LiebUser).FirstOrDefault(x => x.RaidId == raidId && x.LiebUserId == liebUserId && x.SignUpType != SignUpType.Flex);
            if (signUp != null)
            {
                signUp.SignUpType = SignUpType.SignedOff;

                //change and delete Role for Random Raids
                Raid? raid = context.Raids.Include(r => r.Roles).FirstOrDefault(r => r.RaidId == raidId);
                if(raid != null && raid.RaidType != RaidType.Planned && !signUp.RaidRole.IsRandomSignUpRole)
                {
                    context.RaidRoles.Remove(signUp.RaidRole);
                    signUp.RaidRole = raid.Roles.FirstOrDefault(r => r.IsRandomSignUpRole, CreateRandomSignUpRole(raid.RaidType));
                }
                await LogSignUp(signUp, signUp.LiebUser.Name, signedOffByUserId);
            }
            await context.SaveChangesAsync();
            await _discordService.PostRaidMessage(raidId);
        }

        public async Task SignOffExternalUser(int raidId, string userName, ulong signedOffByUserId)
        {
            using var context = _contextFactory.CreateDbContext();
            
            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.ExternalUserName == userName).ToList();
            context.RaidSignUps.RemoveRange(signUps);
            
            await context.SaveChangesAsync();
            RaidSignUp signUp = signUps.FirstOrDefault();
            if(signUp != null)
            {
                signUp.SignUpType = SignUpType.SignedOff;
                await LogSignUp(signUp, userName, signedOffByUserId);
            }
            await _discordService.PostRaidMessage(raidId);
        }

        public async Task ChangeAccount(int raidId, ulong liebUserId, int guildWars2AccountId)
        {
            using var context = _contextFactory.CreateDbContext();
            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId).ToList();
            foreach(RaidSignUp signUp in signUps)
            {
                signUp.GuildWars2AccountId = guildWars2AccountId;
            }
            await context.SaveChangesAsync();
            await _discordService.PostRaidMessage(raidId);
        }

        public async Task ChangeSignUpType(int raidId, ulong liebUserId, int plannedRoleId, SignUpType signUpType, bool postChanges = true)
        {
            if (!IsRoleSignUpAllowed(raidId, liebUserId, plannedRoleId, signUpType, true))
            {
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId).Include(s => s.LiebUser).ToList();

            RaidSignUp? signUp = signUps.FirstOrDefault(x => x.SignUpType != SignUpType.Flex);
            RaidSignUp? flexSignUp = signUps.FirstOrDefault(x => x.SignUpType == SignUpType.Flex && x.RaidRoleId == plannedRoleId);

            //change Flex to current role
            if (flexSignUp != null && signUp != null)
            {
                flexSignUp.RaidRoleId = signUp.RaidRoleId;
            }

            //change to new role
            if (signUp != null)
            {
                signUp.RaidRoleId = plannedRoleId;
                signUp.SignUpType = signUpType;
                if(signUp.IsExternalUser)
                {
                    await LogSignUp(signUp, signUp.ExternalUserName);
                }
                else
                {
                    await LogSignUp(signUp, signUp.LiebUser.Name);
                }
            }
            context.SaveChanges();
            if(postChanges)
            {
                await _discordService.PostRaidMessage(raidId);
            }
        }

        public bool IsRoleSignUpAllowed(ulong liebUserId, int plannedRoleId, SignUpType signUpType)
        {
            if(signUpType == SignUpType.Backup || signUpType == SignUpType.Flex || signUpType == SignUpType.SignedOff)
            {
                return true;
            }

            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(s => s.RaidRoleId == plannedRoleId).ToList();

            RaidRole? role = context.RaidRoles
            .FirstOrDefault(r => r.RaidRoleId == plannedRoleId);

            if (role == null)
                return false;

            if(signUps.Count(s => s.SignUpType == SignUpType.SignedUp) < role.Spots)
                return true;

            return signUps.Where(s => s.LiebUserId == liebUserId && s.SignUpType == SignUpType.SignedUp).Any();
        }

        public bool IsRoleSignUpAllowed(int raidId, ulong liebUserId, int plannedRoleId, SignUpType signUpType, bool moveFlexUser)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
            .Include(r => r.Roles)
            .Include(r => r.SignUps)
            .FirstOrDefault(r => r.RaidId == raidId);

            if (raid == null) return false;

            if (raid.RaidType == RaidType.Planned)
            {
                if (raid.MoveFlexUsers)
                {
                    return IsRoleSignUpAllowed(raid, liebUserId, plannedRoleId, signUpType, moveFlexUser, new List<int>()).Result;
                }
                else
                {
                    return IsRoleSignUpAllowed(liebUserId, plannedRoleId, signUpType);
                }
            }
            else
            {
                RaidRole? role = context.RaidRoles
                    .AsNoTracking()
                    .FirstOrDefault(r => r.RaidRoleId == plannedRoleId);
                if(role == null) return false;
                if (role.IsRandomSignUpRole)
                {
                    // new sign up is available if there are free spots and the user is not signed up or still in the random role
                    RaidSignUp? signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == liebUserId);
                    return raid.SignUps.Where(s => s.SignUpType == SignUpType.SignedUp).Count() < role.Spots
                        && (signUp == null || signUp.RaidRoleId == plannedRoleId || signUp.SignUpType == SignUpType.SignedOff);
                }
                return raid.SignUps.Where(s => s.LiebUserId == liebUserId && s.RaidRoleId == plannedRoleId).Any();
            }
        }

        private async Task<bool> IsRoleSignUpAllowed(Raid raid, ulong liebUserId, int plannedRoleId, SignUpType signUpType, bool moveFlexUser, List<int> checkedRoleIds)
        {
            if (IsRoleSignUpAllowed(liebUserId, plannedRoleId, signUpType))
                return true;

            if (checkedRoleIds == null)
                checkedRoleIds = new List<int>();
            checkedRoleIds.Add(plannedRoleId);

            if (raid == null)
            {
                return false;
            }

            foreach (ulong userId in raid.SignUps.Where(s => s.RaidRoleId == plannedRoleId && s.SignUpType == SignUpType.SignedUp).Select(s => s.LiebUserId))
            {
                foreach (RaidSignUp signUp in raid.SignUps.Where(s => s.LiebUserId == userId && s.SignUpType == SignUpType.Flex))
                {
                    if (!checkedRoleIds.Contains(signUp.RaidRoleId)
                        && await IsRoleSignUpAllowed(raid, userId, signUp.RaidRoleId, SignUpType.SignedUp, moveFlexUser, checkedRoleIds))
                    {
                        if (moveFlexUser)
                        {
                            await ChangeSignUpType(raid.RaidId, userId, signUp.RaidRoleId, SignUpType.SignedUp, false);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsRaidSignUpAllowed(ulong liebUserId, int raidId, out string errorMessage, bool ignoreRole = false)
        {
            errorMessage = string.Empty;
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
                .AsNoTracking()
                .FirstOrDefault(r => r.RaidId == raidId);
            if(raid == null)
            {
                errorMessage = "Raid not found.";
                return false;
            }
            LiebUser? user = context.LiebUsers
                .Include(u => u.RoleAssignments)
                .ThenInclude(a => a.LiebRole)
                .Include(u => u.GuildWars2Accounts)
                .ThenInclude(a => a.EquippedBuilds)
                .AsNoTracking()
                .FirstOrDefault(r => r.Id == liebUserId);
            if (user == null)
            {
                errorMessage = "User not found.";
                return false;
            }

            if (user.GuildWars2Accounts.Count == 0)
            {
                errorMessage = "No Guild Wars 2 account found.";
                return false;
            }

            if (raid.RaidType != RaidType.Planned && !user.GuildWars2Accounts.Where(a => a.EquippedBuilds.Count > 0).Any())
            {
                errorMessage = "No equipped Guild Wars 2 build found.";
                return false;
            }

            if (!ignoreRole && !string.IsNullOrEmpty(raid.RequiredRole) 
                && !user.RoleAssignments.Where(a => a.LiebRole.RoleName == raid.RequiredRole).Any() 
                && raid.FreeForAllTimeUTC.UtcDateTime > DateTimeOffset.UtcNow)
            {
                bool userHasRole = user.RoleAssignments.Where(a => a.LiebRole.RoleName == raid.RequiredRole).Any();
                errorMessage = $"The raid is still locked for {raid.RequiredRole}.";
                return false;
            }

            if(raid.EndTimeUTC < DateTimeOffset.UtcNow)
            {
                errorMessage = $"The raid already ended.";
                return false;
            }

            return true;
        }

        public bool IsExternalSignUpAllowed(int raidId, out string errorMessage)
        {
            errorMessage = string.Empty;
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
                .AsNoTracking()
                .FirstOrDefault(r => r.RaidId == raidId);
            if(raid == null)
            {
                errorMessage = "Raid not found.";
                return false;
            }

            if (raid.RaidType != RaidType.Planned)
            {
                errorMessage = "Random raids need an Account with equipped builds.";
                return false;
            }

            if(raid.EndTimeUTC < DateTimeOffset.UtcNow)
            {
                errorMessage = $"The raid already ended.";
                return false;
            }

            return true;
        }

        private async Task LogSignUp(RaidSignUp signUp, string userName, ulong signedUpBy = 0)
        {
            ulong userId = signedUpBy > 0 ? signedUpBy : signUp.LiebUserId.Value;
            RaidLog log = RaidLog.CreateSignUpLog(userId, signUp, userName);

            using var context = _contextFactory.CreateDbContext();
            await context.RaidLogs.AddAsync(log);
            await context.SaveChangesAsync();
            
            await SendDiscordSignUpLogMessage(signUp, userName, signedUpBy);
        }

        public async Task SendDiscordSignUpLogMessage(RaidSignUp signUp, string userName, ulong signedUpBy = 0)
        {
            using var context = _contextFactory.CreateDbContext();

            Raid raid = context.Raids
                .Include(r => r.DiscordRaidMessages)
                .ToList()
                .FirstOrDefault(r => r.RaidId == signUp.RaidId, new Raid());

            if(raid.DiscordRaidMessages.Count > 0)
            {
                string signedUpByUserName = userName;
                if(signedUpBy > 0)
                {
                    LiebUser signedUpByUser = context.LiebUsers
                        .ToList()
                        .FirstOrDefault(u => u.Id == signedUpBy, new LiebUser());
                    signedUpByUserName = signedUpByUser.Name;
                }

                string message = $"{raid.Title}: {signedUpByUserName} signed up {userName} as {signUp.SignUpType.ToString()}";
                foreach(DiscordRaidMessage discordMessage in raid.DiscordRaidMessages)
                {
                    DiscordSettings settings = _discordService.GetDiscordSettings(discordMessage.DiscordGuildId);
                    if(settings.DiscordLogChannel > 0)
                    {
                        await _discordService.SendChannelMessage(discordMessage.DiscordGuildId, settings.DiscordLogChannel, message);
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        public async Task SendReminders()
        {
            using var context = _contextFactory.CreateDbContext();
            List<RaidReminder> reminders = context.RaidReminders
                .Include(r => r.Raid)
                .ThenInclude(r => r.SignUps)
                .Where(r => !r.Sent)
                .ToList();

            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            foreach(RaidReminder reminder in reminders.Where(r => r.ReminderTimeUTC < utcNow))
            {
                switch(reminder.Type)
                {
                    case RaidReminder.ReminderType.User:
                        await _discordService.SendUserReminder(reminder, reminder.Raid);
                        break;
                    case RaidReminder.ReminderType.Channel:
                        await _discordService.SendChannelReminder(reminder);
                        break;
                }
            }
        }

        public RaidRole CreateRandomSignUpRole(RaidType raidType)
        {
            return new RaidRole()
                {
                    Spots = 10,
                    Name = "Random",
                    Description = raidType.ToString(),
                    IsRandomSignUpRole = true
                };
        }
    }
}
