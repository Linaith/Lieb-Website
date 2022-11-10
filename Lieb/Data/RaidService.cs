﻿using Lieb.Models;
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
                .Include(r => r.SignUpHistory)
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
                .Include(r => r.SignUpHistory)
                .Include(r => r.Reminders)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.LiebUser)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.GuildWars2Account)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.RaidRole)
                .Include(r => r.DiscordRaidMessages)
                .FirstOrDefault(r => r.RaidId == raidId);
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
                        RaidRole randomRole = raid.Roles.FirstOrDefault(r => r.IsRandomSignUpRole);
                        foreach (RaidSignUp signUp in raid.SignUps)
                        {
                            signUp.RaidRole = randomRole;
                        }
                        context.RaidRoles.RemoveRange(raid.Roles.Where(r => !r.IsRandomSignUpRole));
                    }

                    await context.SaveChangesAsync();
                }
                _discordService.PostRaidMessage(raid.RaidId);
            }
        }

        public async Task DeleteRaid(int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid raid = GetRaid(raidId);
            context.RaidSignUps.RemoveRange(raid.SignUps);
            context.RaidRoles.RemoveRange(raid.Roles);
            context.RaidSignUpHistories.RemoveRange(raid.SignUpHistory);
            context.RaidReminders.RemoveRange(raid.Reminders);
            await context.SaveChangesAsync();
            context.Raids.Remove(raid);
            await context.SaveChangesAsync();
            _discordService.DeleteRaidMessages(raid);
        }

        public async Task SignUp(int raidId, ulong liebUserId, int guildWars2AccountId, int plannedRoleId, SignUpType signUpType)
        {
            if (!IsRoleSignUpAllowed(raidId, liebUserId, plannedRoleId, signUpType, true))
            {
                return;
            }
            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(r => r.RaidId == raidId && r.LiebUserId == liebUserId).ToList();
            if (signUpType != SignUpType.Flex && signUps.Where(r => r.SignUpType != SignUpType.Flex).Any())
            {
                ChangeSignUpType(raidId, liebUserId, plannedRoleId, signUpType);
            }
            else if (!signUps.Where(r => r.RaidRoleId == plannedRoleId).Any())
            {
                context.RaidSignUps.Add(new RaidSignUp()
                {
                    GuildWars2AccountId = guildWars2AccountId,
                    RaidId = raidId,
                    LiebUserId = liebUserId,
                    RaidRoleId = plannedRoleId,
                    SignUpType = signUpType
                });
                await context.SaveChangesAsync();
            }
            _discordService.PostRaidMessage(raidId);
        }

        public async Task SignOff(int raidId, ulong liebUserId)
        {
            using var context = _contextFactory.CreateDbContext();
            //remove Flex Sign Ups
            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId && x.SignUpType == SignUpType.Flex).ToList();
            context.RaidSignUps.RemoveRange(signUps);

            //change to SignedOff
            RaidSignUp? signUp = context.RaidSignUps.FirstOrDefault(x => x.RaidId == raidId && x.LiebUserId == liebUserId && x.SignUpType != SignUpType.Flex);
            if (signUp != null)
            {
                signUp.SignUpType = SignUpType.SignedOff;

                //change and delete Role for Random Raids
                Raid? raid = context.Raids.Include(r => r.Roles).FirstOrDefault(r => r.RaidId == raidId);
                if(raid != null && raid.RaidType != RaidType.Planned && !signUp.RaidRole.IsRandomSignUpRole)
                {
                    context.RaidRoles.Remove(signUp.RaidRole);
                    signUp.RaidRole = raid.Roles.FirstOrDefault(r => r.IsRandomSignUpRole);
                }
            }
            await context.SaveChangesAsync();
            _discordService.PostRaidMessage(raidId);
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
            _discordService.PostRaidMessage(raidId);
        }

        public void ChangeSignUpType(int raidId, ulong liebUserId, int plannedRoleId, SignUpType signUpType)
        {
            if (!IsRoleSignUpAllowed(raidId, liebUserId, plannedRoleId, signUpType, true))
            {
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId).ToList();

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
            }
            context.SaveChanges();
            _discordService.PostRaidMessage(raidId);
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
                            ChangeSignUpType(raid.RaidId, userId, signUp.RaidRoleId, SignUpType.SignedUp);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsRaidSignUpAllowed(ulong liebUserId, int raidId, out string errorMessage)
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

            if (!string.IsNullOrEmpty(raid.RequiredRole) 
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

        public void SendReminders()
        {
            using var context = _contextFactory.CreateDbContext();
            List<Raid> raids = context.Raids
                .Include(r => r.Reminders)
                .ToList();
            
            foreach(Raid raid in raids)
            {
                foreach(RaidReminder reminder in raid.Reminders.Where(reminder => !reminder.Sent && raid.StartTimeUTC.AddHours(-reminder.HoursBeforeRaid) < DateTime.UtcNow))
                {
                    //TODO send reminders -> this is a Discord Problem...
                }
            }
        }
    }
}
