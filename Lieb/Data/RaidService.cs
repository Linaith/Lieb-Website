using Lieb.Models;
using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Lieb.Models.Poll;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class RaidService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private readonly DiscordService _discordService;
        private readonly PollService _pollService;

        public RaidService(IDbContextFactory<LiebContext> contextFactory, DiscordService discordService, PollService pollService)
        {
            _contextFactory = contextFactory;
            _discordService = discordService;
            _pollService = pollService;
        }

        public List<Raid> GetRaids()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Raids
                .Include(r => r.Roles)
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
            Raid raid = context.Raids
                .Include(r => r.Roles)
                .Include(r => r.Reminders)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.LiebUser)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.GuildWars2Account)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.RaidRole)
                .Include(r => r.DiscordRaidMessages)
                .FirstOrDefault(r => r.RaidId == raidId);
            if(raid != null) return raid;
            else return new Raid();
        }

        public async Task AddOrEditRaid(Raid raid, List<RaidRole> rolesToDelete, List<RaidReminder> remindersToDelete, List<DiscordRaidMessage> messagesToDelete, ulong? changedBy)
        {
            if (raid != null)
            {
                using var context = _contextFactory.CreateDbContext();
                if (raid.RaidId == 0)
                {
                    context.Raids.Add(raid);
                }
                else
                {
                    context.Update(raid);
                    context.RaidRoles.RemoveRange(rolesToDelete);
                    context.RaidReminders.RemoveRange(remindersToDelete);
                    context.DiscordRaidMessages.RemoveRange(messagesToDelete);
                    await _discordService.DeleteRaidMessages(messagesToDelete);

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
                }
                await context.SaveChangesAsync();
                await _discordService.PostRaidMessage(raid.RaidId);
            }
        }

        public async Task DeleteRaid(int raidId, ulong? userId)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid raid = GetRaid(raidId);
            await _discordService.DeleteRaidMessages(raid);
            if(raid.EndTimeUTC > DateTimeOffset.UtcNow)
            {
                await _discordService.SendMessageToRaidUsers($"Raid \"{raid.Title}\": was deleted.", raid);
            }

            context.RaidSignUps.RemoveRange(raid.SignUps);
            context.RaidRoles.RemoveRange(raid.Roles);
            context.RaidReminders.RemoveRange(raid.Reminders);
            context.DiscordRaidMessages.RemoveRange(raid.DiscordRaidMessages);
            await context.SaveChangesAsync();

            raid.SignUps.Clear();
            raid.Roles.Clear();
            raid.Reminders.Clear();
            raid.DiscordRaidMessages.Clear();
            context.Raids.Remove(raid);
            await context.SaveChangesAsync();
        }

        public async Task<bool> SignUp(int raidId, ulong liebUserId, int guildWars2AccountId, int plannedRoleId, SignUpType signUpType, ulong signedUpByUserId = 0)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
            .Include(r => r.Roles)
            .Include(r => r.SignUps)
            .FirstOrDefault(r => r.RaidId == raidId);
            if(raid == null) return false;

            LiebUser user = context.LiebUsers.FirstOrDefault(l => l.Id == liebUserId);
            if(user == null) return false;

            bool result = false;
            if(raid.RaidType == RaidType.Planned)
            {
                result = await SignUpPlanned(raidId, liebUserId, guildWars2AccountId, plannedRoleId, signUpType, user.Name, signedUpByUserId);
            }
            else
            {
                result = await SignUpRandom(raidId, liebUserId, guildWars2AccountId, signUpType, user.Name, signedUpByUserId);
            }

            user.LastSignUpAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            await _discordService.PostRaidMessage(raidId);
            return result;
        }

        private async Task<bool> SignUpPlanned(int raidId, ulong liebUserId, int guildWars2AccountId, int plannedRoleId, SignUpType signUpType, string userName, ulong signedUpByUserId = 0)
        {
            if (!IsRoleSignUpAllowed(raidId, liebUserId, plannedRoleId, signUpType, true))
            {
                return false;
            }

            using var context = _contextFactory.CreateDbContext();


            List<RaidSignUp> signUps = context.RaidSignUps.Where(r => r.RaidId == raidId && r.LiebUserId == liebUserId).ToList();
            if (signUpType != SignUpType.Flex && signUps.Where(r => r.SignUpType != SignUpType.Flex).Any())
            {
                await ChangeSignUpType(raidId, liebUserId, signUpType, false);
                await ChangeRole(raidId, liebUserId, plannedRoleId, signUpType, RaidType.Planned, false);
                await ChangeAccount(raidId, liebUserId, guildWars2AccountId, false);
            }
            else if (!signUps.Where(r => r.RaidRoleId == plannedRoleId).Any())
            {
                RaidSignUp signUp = new RaidSignUp(raidId, liebUserId, guildWars2AccountId, plannedRoleId, signUpType);
                context.RaidSignUps.Add(signUp);
                await context.SaveChangesAsync();
                await LogSignUp(signUp, userName, signedUpByUserId);
            }
            else
            {
                return false;
            }
            foreach(Poll poll in context.Polls.Where(p => p.RaidId == raidId && !p.Answers.Where(a => a.UserId == liebUserId).Any()))
            {
                await _pollService.AddUser(poll.PollId, liebUserId);
            }
            return true;
        }

        private async Task<bool> SignUpRandom(int raidId, ulong liebUserId, int guildWars2AccountId, SignUpType signUpType, string userName, ulong signedUpByUserId = 0)
        {
            using var context = _contextFactory.CreateDbContext();
            if(signUpType == SignUpType.Flex) return false;

            Raid? raid = context.Raids
                .Include(r => r.Roles)
                .Include(r => r.SignUps)
                .FirstOrDefault(r => r.RaidId == raidId);

            GuildWars2Account account = context.GuildWars2Accounts
                                            .Include(a => a.EquippedBuilds)
                                            .ThenInclude(b => b.GuildWars2Build)
                                            .FirstOrDefault(a => a.GuildWars2AccountId == guildWars2AccountId);

            if(account == null) return false;
            if(!account.EquippedBuilds.Where(b => b.GuildWars2Build.UseInRandomRaid).Any()) return false;
            
            if(IsRandomRoleSignUpAllowed(raid, liebUserId, signUpType))
            {
                RaidSignUp signUp;
                if(raid.SignUps.Where(s => s.LiebUserId == liebUserId).Any())
                {
                    signUp = raid.SignUps.First(s => s.LiebUserId == liebUserId);
                    signUp.SignUpType = signUpType;
                }
                else
                {
                    RaidRole randomRole = raid.Roles.First(r => r.IsRandomSignUpRole);
                    signUp = new RaidSignUp(raid.RaidId, liebUserId, guildWars2AccountId, randomRole.RaidRoleId , signUpType);
                    context.RaidSignUps.Add(signUp);
                }
                await context.SaveChangesAsync();
                foreach(Poll poll in context.Polls.Where(p => p.RaidId == raidId && !p.Answers.Where(a => a.UserId == liebUserId).Any()))
                {
                    await _pollService.AddUser(poll.PollId, liebUserId);
                }
                await LogSignUp(signUp, userName, signedUpByUserId);
                return true;
            }
            return false;
        }

        public async Task<bool> SignUpExternalUser(int raidId, string userName, int plannedRoleId, SignUpType signUpType, ulong signedUpByUserId)
        {
            if (!IsRoleSignUpAllowed(raidId, ulong.MaxValue, plannedRoleId, signUpType, true))
            {
                return false;
            }
            using var context = _contextFactory.CreateDbContext();

            
            RaidSignUp signUp = new RaidSignUp(raidId, userName, plannedRoleId, signUpType);
            context.RaidSignUps.Add(signUp);
            await context.SaveChangesAsync();
            await LogSignUp(signUp, userName, signedUpByUserId);
            await _discordService.PostRaidMessage(raidId);
            return true;
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
                await LogSignUp(signUp, signUp.LiebUser.Name, signedOffByUserId);
            }
            await context.SaveChangesAsync();
            foreach(Poll poll in context.Polls.Where(p => p.RaidId == raidId && !p.Answers.Where(a => a.UserId == liebUserId).Any()))
            {
                await _pollService.RemoveUser(poll.PollId, liebUserId);
            }
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

        public async Task ChangeAccount(int raidId, ulong liebUserId, int guildWars2AccountId, bool postChanges = true)
        {
            using var context = _contextFactory.CreateDbContext();
            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId).ToList();
            foreach(RaidSignUp signUp in signUps)
            {
                signUp.GuildWars2AccountId = guildWars2AccountId;
            }
            await context.SaveChangesAsync();
            if(postChanges)
            {
                await _discordService.PostRaidMessage(raidId);
            }
        }

        public async Task ChangeRole(int raidId, ulong liebUserId, int plannedRoleId, SignUpType signUpType, RaidType raidType, bool postChanges = true)
        {
            if (raidType != RaidType.Planned || !IsRoleSignUpAllowed(raidId, liebUserId, plannedRoleId, signUpType, true))
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
                if(!signUp.IsExternalUser)
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

        public async Task ChangeSignUpType(int raidId, ulong liebUserId, SignUpType signUpType, bool postChanges = true)
        {
            using var context = _contextFactory.CreateDbContext();

            RaidSignUp? signUp = context.RaidSignUps
                                        .Include(s => s.LiebUser)
                                        .FirstOrDefault(x => x.RaidId == raidId && x.LiebUserId == liebUserId 
                                                        && x.SignUpType != SignUpType.Flex);
            
            if (signUp != null && IsRoleSignUpAllowed(raidId, liebUserId, signUp.RaidRoleId, signUpType, true))
            {
                signUp.SignUpType = signUpType;
                await LogSignUp(signUp, signUp.LiebUser.Name);
                context.SaveChanges();
            }
            if(postChanges)
            {
                await _discordService.PostRaidMessage(raidId);
            }
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
                    //moveFlexUser parameter is for only checking or really moving
                    return IsRoleSignUpAllowedMoveFlex(raid, liebUserId, plannedRoleId, signUpType, moveFlexUser, new List<int>()).Result;
                }
                else
                {
                    return IsRoleSignUpAllowedBasicCheck(liebUserId, plannedRoleId, signUpType);
                }
            }
            else
            {
                return IsRandomRoleSignUpAllowed(raid, liebUserId, signUpType, plannedRoleId);
            }
        }

        private bool IsRoleSignUpAllowedBasicCheck(ulong liebUserId, int plannedRoleId, SignUpType signUpType)
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

        private async Task<bool> IsRoleSignUpAllowedMoveFlex(Raid raid, ulong liebUserId, int plannedRoleId, SignUpType signUpType, bool moveFlexUser, List<int> checkedRoleIds)
        {
            if (IsRoleSignUpAllowedBasicCheck(liebUserId, plannedRoleId, signUpType))
                return true;

            if (checkedRoleIds == null)
                checkedRoleIds = new List<int>();
            checkedRoleIds.Add(plannedRoleId);

            if (raid == null)
            {
                return false;
            }

            foreach (ulong userId in raid.SignUps.Where(s => s.RaidRoleId == plannedRoleId && s.SignUpType == SignUpType.SignedUp && s.LiebUserId.HasValue).Select(s => s.LiebUserId))
            {
                foreach (RaidSignUp signUp in raid.SignUps.Where(s => s.LiebUserId == userId && s.SignUpType == SignUpType.Flex))
                {
                    if (!checkedRoleIds.Contains(signUp.RaidRoleId)
                        && await IsRoleSignUpAllowedMoveFlex(raid, userId, signUp.RaidRoleId, SignUpType.SignedUp, moveFlexUser, checkedRoleIds))
                    {
                        if (moveFlexUser)
                        {
                            await ChangeRole(raid.RaidId, userId, signUp.RaidRoleId, SignUpType.SignedUp, raid.RaidType, false);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsRandomRoleSignUpAllowed(Raid raid, ulong liebUserId, SignUpType signUpType, int plannedRoleId = 0)
        {
            RaidRole randomRole = raid.Roles.First(r => r.IsRandomSignUpRole);
            int signedUpUsers = raid.SignUps.Where(s => s.SignUpType == SignUpType.SignedUp).Count();
            int randomSpots = randomRole.Spots;
            
            if(raid.SignUps.Where(s => s.LiebUserId == liebUserId).Any())
            {
                int userRoleId = raid.SignUps.First(s => s.LiebUserId == liebUserId).RaidRoleId;
                if(plannedRoleId != 0 && plannedRoleId != userRoleId) return false;
            }
            else
            {
                if(plannedRoleId != 0 && plannedRoleId != randomRole.RaidRoleId) return false;
            }

            if(randomSpots - signedUpUsers > 0) return true;

            if(signUpType == SignUpType.Backup) return true;

            if(raid.SignUps.Where(s => s.LiebUserId == liebUserId).Any())
            {
                return raid.SignUps.First(s => s.LiebUserId == liebUserId).SignUpType == SignUpType.SignedUp;
            }
            return false;
        }

        public bool IsRaidSignUpAllowed(ulong liebUserId, int raidId, out string errorMessage, bool ignoreRole = false, bool discordErrorMessage = false)
        {
            errorMessage = string.Empty;
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
                .Include(r => r.SignUps)
                .AsNoTracking()
                .FirstOrDefault(r => r.RaidId == raidId);
            if(raid == null)
            {
                errorMessage = "Raid not found.";
                return false;
            }

            if (!raid.HasEnoughUsers && raid.MinUserDeadLineUTC < DateTimeOffset.UtcNow)
            {
                errorMessage = $"The raid was canceled because of not enough sign ups.";
                return false;
            }

            LiebUser? user = context.LiebUsers
                .Include(u => u.RoleAssignments)
                .ThenInclude(a => a.LiebRole)
                .Include(u => u.GuildWars2Accounts)
                .ThenInclude(a => a.EquippedBuilds)
                .ThenInclude(b => b.GuildWars2Build)
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

            if (raid.RaidType != RaidType.Planned && !user.GuildWars2Accounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.UseInRandomRaid).Count() > 0).Any())
            {
                if(discordErrorMessage)
                    errorMessage = "No equipped Guild Wars 2 build found. Please add your builds at https://lieb.games";
                else
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

            if(user.BannedUntil > DateTimeOffset.UtcNow)
            {
                errorMessage = $"You are banned until {user.BannedUntil}.";
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

        public bool IsRaidSlashCommandAllowed(ulong liebUserId, out string errorMessage)
        {
            errorMessage = string.Empty;

            using var context = _contextFactory.CreateDbContext();
            LiebUser user = context.LiebUsers
                                .Include(u => u.RoleAssignments)
                                .ThenInclude(a => a.LiebRole)
                                .FirstOrDefault(u => u.Id == liebUserId);
            if (user != null && user.RoleAssignments.Max(a => a.LiebRole.Level) >= Constants.Roles.RaidLead.PowerLevel)
            {
                return true;
            }
            errorMessage = "insufficient permissions";
            return false;
        }

        private async Task LogSignUp(RaidSignUp signUp, string userName, ulong signedUpBy = 0)
        {
            _ = SendDiscordSignUpLogMessage(signUp, userName, signedUpBy);
        }

        public async Task SendDiscordSignUpLogMessage(RaidSignUp signUp, string userName, ulong signedUpBy = 0)
        {
            using var context = _contextFactory.CreateDbContext();

            Raid raid = context.Raids
                .Include(r => r.DiscordRaidMessages)
                .FirstOrDefault(r => r.RaidId == signUp.RaidId);

            if(raid == null) return;

            if(raid.DiscordRaidMessages.Count > 0)
            {
                string signedUpByUserName = userName;
                if(signedUpBy > 0)
                {
                    LiebUser signedUpByUser = context.LiebUsers
                        .FirstOrDefault(u => u.Id == signedUpBy);

                    if(signedUpByUser != null)
                        signedUpByUserName = signedUpByUser.Name;
                    else
                        signedUpByUserName = "user not found";
                }

                string message = $"{signedUpByUserName} signed up {userName} as {signUp.SignUpType.ToString()}";
                HashSet<ulong> guildIds = raid.DiscordRaidMessages.Select(m => m.DiscordGuildId).ToHashSet();
                foreach(ulong guildId in guildIds)
                {
                    DiscordSettings settings = _discordService.GetDiscordSettings(guildId);
                    if(settings.DiscordLogChannel > 0)
                    {
                        await _discordService.SendChannelMessage(guildId, settings.DiscordLogChannel, message, raid.Title);
                    }
                }
            }
        }

        public async Task SendReminders()
        {
            using var context = _contextFactory.CreateDbContext();
            List<RaidReminder> reminders = context.RaidReminders
                .Where(r => !r.Sent)
                .ToList();

            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            foreach(RaidReminder reminder in reminders.Where(r => r.ReminderTimeUTC < utcNow))
            {
                Raid? raid = context.Raids
                    .Include(r => r.SignUps)
                    .Include(r => r.Reminders)
                    .FirstOrDefault(r => r.Reminders.Where(re => re.RaidReminderId == reminder.RaidReminderId).Any());
                if (raid != null)
                {
                    switch (reminder.Type)
                    {
                        case RaidReminder.ReminderType.User:
                            await _discordService.SendUserReminder(reminder, raid);
                            break;
                        case RaidReminder.ReminderType.Channel:
                            await _discordService.SendChannelReminder(reminder, raid.Title);
                            break;
                        case RaidReminder.ReminderType.Group:
                            await _discordService.SendGroupReminder(reminder, raid);
                            break;
                    }
                }
            }
        }

        public RaidRole CreateRandomSignUpRole(RaidType raidType, int spots = 10)
        {
            return new RaidRole()
                {
                    Spots = spots,
                    Name = "Random",
                    Description = raidType.ToString(),
                    IsRandomSignUpRole = true
                };
        }

        public async Task RemoveMaybes()
        {
            using var context = _contextFactory.CreateDbContext();
            List<Raid> raids = context.Raids.Include(r => r.SignUps).ToList();
            foreach(Raid raid in raids.Where(r => r.StartTimeUTC < DateTimeOffset.UtcNow.AddMinutes(Constants.REMOVE_MAYBE_MINUTES)
                                                && r.SignUps.Where(s => s.SignUpType == SignUpType.Maybe).Any()))
            {
                foreach(RaidSignUp signup in raid.SignUps)
                {
                    if(signup.SignUpType == SignUpType.Maybe && signup.LiebUserId.HasValue)
                    {
                        await SignOff(raid.RaidId, signup.LiebUserId.Value);
                    }
                }
            }
        }

        public async Task CleanUpRaids()
        {
            using var context = _contextFactory.CreateDbContext();
            List<Raid> raids = GetRaids();

            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            foreach(Raid raid in raids.Where(r => r.EndTimeUTC < utcNow.AddYears(-1)))
            {
                await DeleteRaid(raid.RaidId, null);
            }
            foreach(Raid raid in raids.Where(r => r.EndTimeUTC < utcNow.AddHours(-1) 
                                                && (r.DiscordRaidMessages.Count > 0 || r.Reminders.Count > 0)))
            {
                await _discordService.DeleteRaidMessages(raid);
                context.RaidReminders.RemoveRange(raid.Reminders);
                context.DiscordRaidMessages.RemoveRange(raid.DiscordRaidMessages);
                await context.SaveChangesAsync();
            }
        }

        public async Task CheckMinUsers()
        {
            using var context = _contextFactory.CreateDbContext();
            List<Raid> raids = context.Raids
                                .Include(r => r.SignUps)
                                .Where(r => r.MinUserPollId == null).ToList();
            foreach (Raid raid in raids.Where(r => !r.HasEnoughUsers && r.MinUserDeadLineUTC < DateTimeOffset.UtcNow && r.StartTimeUTC > DateTimeOffset.UtcNow))
            {
                raid.MinUserPollId = await _pollService.CreatePoll(
                                        "The raid has not the required users, do you want to raid anyway?", 
                                        new List<string>() {Constants.Polls.YES, Constants.Polls.NO }, raid.RaidId, true);
                await context.SaveChangesAsync();
                await _discordService.PostRaidMessage(raid.RaidId);
            }
        }

        public async Task CheckMinUserPollResult()
        {
            using var context = _contextFactory.CreateDbContext();
            List<Raid> raids = context.Raids
                                .Include(r => r.SignUps)
                                .Where(r => r.MinUserPollId != null).ToList();
            foreach (Raid raid in raids.Where(r => !r.HasEnoughUsers && r.MinUserDeadLineUTC < DateTimeOffset.UtcNow && r.StartTimeUTC > DateTimeOffset.UtcNow))
            {
                Poll poll = _pollService.GetPoll(raid.MinUserPollId.Value);

                if (poll.Answers.Count == 0) continue;

                //continue if not everything is answered and the poll is not older than 1 day
                if (poll.Answers.Where(a => string.IsNullOrEmpty(a.Answer)).Any()
                        && poll.CreatedAt > DateTimeOffset.UtcNow.AddDays(-1)) continue;

                //sign off users that answered "No"
                HashSet<ulong> signedOffUsers = poll.Answers.Where(a => a.Answer == Constants.Polls.NO || string.IsNullOrEmpty(a.Answer)).Select(a => a.UserId).ToHashSet();
                foreach(ulong user in signedOffUsers)
                {
                    await SignOff(raid.RaidId, user);
                }
                await _discordService.SendMessageToUsers("You have been signed off.", raid.Title, signedOffUsers);

                //reopen the raid
                raid.MinUsers = 0;
                await context.SaveChangesAsync();
                await _discordService.SendMessageToRaidUsers("People who have voted \"No\" have been signed off. Signing up is allowed again.", raid);
                await _discordService.PostRaidMessage(raid.RaidId);
            }
        }
    }
}
