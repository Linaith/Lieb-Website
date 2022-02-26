﻿using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class RaidService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;

        public RaidService(IDbContextFactory<LiebContext> contextFactory)
        {
            _contextFactory = contextFactory;
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
                .ThenInclude(s => s.PlannedRaidRole)
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
                .ThenInclude(s => s.PlannedRaidRole)
                .FirstOrDefault(r => r.RaidId == raidId);
        }

        public async Task AddOrEditRaid(Raid raid)
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
                    Raid raidToChange = await context.Raids
                        .Include(r => r.Roles)
                        .Include(r => r.SignUpHistory)
                        .Include(r => r.Reminders)
                        .Include(r => r.SignUps)
                        .FirstOrDefaultAsync(r => r.RaidId == raid.RaidId);
                    raidToChange.Title = raid.Title;
                    raidToChange.Description = raid.Description;
                    raidToChange.Date = raid.Date;
                    raidToChange.StartTime = raid.StartTime;
                    raidToChange.EndTime = raid.EndTime;
                    raidToChange.Organizer = raid.Organizer;
                    raidToChange.Guild = raid.Guild;
                    raidToChange.VoiceChat = raid.VoiceChat;
                    raidToChange.RaidType = raid.RaidType;
                    raidToChange.Frequency = raid.Frequency;
                    raidToChange.DiscordMessageId = raid.DiscordMessageId;
                    raidToChange.DiscordChannelId = raid.DiscordChannelId;
                    raidToChange.DiscordGuildId = raid.DiscordGuildId;

                    foreach(PlannedRaidRole role in raidToChange.Roles)
                    {
                        PlannedRaidRole? newRole = raid.Roles.FirstOrDefault(r => r.PlannedRaidRoleId == role.PlannedRaidRoleId);
                        if(newRole != null)
                        {
                            role.Spots = newRole.Spots;
                            role.Name = newRole.Name;
                            role.Description = newRole.Description;
                        }
                        else
                        {
                            raidToChange.Roles.Remove(role);
                            context.PlannedRaidRoles.Remove(role);
                        }
                    }
                    foreach (PlannedRaidRole role in raid.Roles.Where(r => r.PlannedRaidRoleId == 0))
                    {
                        raidToChange.Roles.Add(role);
                    }

                    foreach (RaidReminder reminder in raidToChange.Reminders)
                    {
                        RaidReminder? newReminder = raid.Reminders.FirstOrDefault(r => r.RaidReminderId == reminder.RaidReminderId);
                        if (newReminder != null)
                        {
                            reminder.Type = newReminder.Type;
                            reminder.Message = newReminder.Message;
                            reminder.HoursBeforeRaid = newReminder.HoursBeforeRaid;
                            reminder.ChannelId = newReminder.ChannelId;
                            reminder.Sent = newReminder.Sent;
                        }
                        else
                        {
                            raidToChange.Reminders.Remove(reminder);
                            context.RaidReminders.Remove(reminder);
                        }
                    }
                    foreach (PlannedRaidRole role in raid.Roles.Where(r => r.PlannedRaidRoleId == 0))
                    {
                        raidToChange.Roles.Add(role);
                    }

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteRaid(int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid raid = GetRaid(raidId);
            context.RaidSignUps.RemoveRange(raid.SignUps);
            context.PlannedRaidRoles.RemoveRange(raid.Roles);
            context.SignUpHistories.RemoveRange(raid.SignUpHistory);
            context.RaidReminders.RemoveRange(raid.Reminders);
            await context.SaveChangesAsync();
            context.Raids.Remove(raid);
            await context.SaveChangesAsync();
        }

        public async Task SignUp(int raidId, int liebUserId, int guildWars2AccountId, int plannedRoleId, SignUpType signUpType)
        {
            if (!IsSignUpAllowed(liebUserId, plannedRoleId, signUpType))
            {
                return;
            }
            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(r => r.RaidId == raidId && r.LiebUserId == liebUserId).ToList();
            if (signUpType != SignUpType.Flex && signUps.Where(r => r.SignUpType != SignUpType.Flex).Any())
            {
                await ChangeSignUpType(raidId, liebUserId, plannedRoleId, signUpType);
            }
            else if (!signUps.Where(r => r.PlannedRaidRoleId == plannedRoleId).Any())
            {
                context.RaidSignUps.Add(new RaidSignUp()
                {
                    GuildWars2AccountId = guildWars2AccountId,
                    RaidId = raidId,
                    LiebUserId = liebUserId,
                    PlannedRaidRoleId = plannedRoleId,
                    SignUpType = signUpType
                });
                await context.SaveChangesAsync();
            }
        }

        public async Task SignOff(int raidId, int liebUserId, int plannedRoleId)
        {
            await ChangeSignUpType(raidId, liebUserId, plannedRoleId, SignUpType.SignedOff);
            using var context = _contextFactory.CreateDbContext();
            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId && x.SignUpType == SignUpType.Flex).ToList();
            context.RaidSignUps.RemoveRange(signUps);
            await context.SaveChangesAsync();
        }

        public async Task ChangeAccount(int raidId, int liebUserId, int guildWars2AccountId)
        {
            using var context = _contextFactory.CreateDbContext();
            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId).ToList();
            foreach(RaidSignUp signUp in signUps)
            {
                signUp.GuildWars2AccountId = guildWars2AccountId;
            }
            await context.SaveChangesAsync();
        }

        public async Task ChangeSignUpType(int raidId, int liebUserId, int plannedRoleId, SignUpType signUpType)
        {
            if (!IsSignUpAllowed(liebUserId, plannedRoleId, signUpType))
            {
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId).ToList();

            RaidSignUp? signUp = signUps.FirstOrDefault(x => x.SignUpType != SignUpType.Flex);
            RaidSignUp? flexSignUp = signUps.FirstOrDefault(x => x.SignUpType == SignUpType.Flex && x.PlannedRaidRoleId == plannedRoleId);

            //change Flex to current role
            if (flexSignUp != null && signUp != null)
            {
                flexSignUp.PlannedRaidRoleId = signUp.PlannedRaidRoleId;
                await context.SaveChangesAsync();
            }

            //change to new role
            if (signUp != null)
            {
                signUp.PlannedRaidRoleId = plannedRoleId;
                signUp.SignUpType = signUpType;
                await context.SaveChangesAsync();
            }
        }

        public bool IsSignUpAllowed(int liebUserId, int plannedRoleId, SignUpType signUpType)
        {
            if(signUpType == SignUpType.Backup || signUpType == SignUpType.Flex || signUpType == SignUpType.SignedOff)
            {
                return true;
            }

            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(s => s.PlannedRaidRoleId == plannedRoleId).ToList();

            PlannedRaidRole? role = context.PlannedRaidRoles
            .FirstOrDefault(r => r.PlannedRaidRoleId == plannedRoleId);

            if (role == null)
                return false;

            if(signUps.Count(s => s.SignUpType == SignUpType.SignedUp) < role.Spots)
                return true;

            return signUps.Where(s => s.LiebUserId == liebUserId && s.SignUpType == SignUpType.SignedUp).Any();
        }

        public async Task<bool> IsSignUpAllowed(int raidId, int liebUserId, int plannedRoleId, SignUpType signUpType, bool moveFlexUser, List<int> checkedRoleIds)
        {
            if (IsSignUpAllowed(liebUserId, plannedRoleId, signUpType))
                return true;

            if (checkedRoleIds == null)
                checkedRoleIds = new List<int>();
            checkedRoleIds.Add(plannedRoleId);

            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
            .Include(r => r.Roles)
            .Include(r => r.SignUps)
            .FirstOrDefault(r => r.RaidId == raidId);

            if (raid == null)
            {
                return false;
            }

            foreach (int userId in raid.SignUps.Where(s => s.PlannedRaidRoleId == plannedRoleId && s.SignUpType == SignUpType.SignedUp).Select(s => s.LiebUserId))
            {
                foreach (RaidSignUp signUp in raid.SignUps.Where(s => s.LiebUserId == userId && s.SignUpType == SignUpType.Flex))
                {
                    if (!checkedRoleIds.Contains(signUp.PlannedRaidRoleId)
                        && await IsSignUpAllowed(raidId, userId, signUp.PlannedRaidRoleId, SignUpType.SignedUp, moveFlexUser, checkedRoleIds))
                    {
                        if (moveFlexUser)
                        {
                            await ChangeSignUpType(raidId, userId, signUp.PlannedRaidRoleId, SignUpType.SignedUp);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
}