﻿using Lieb.Models;
using Lieb.Models.GuildWars2.Raid;
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
                    Raid? raidToChange = await context.Raids
                        .Include(r => r.Roles)
                        .Include(r => r.SignUpHistory)
                        .Include(r => r.Reminders)
                        .Include(r => r.SignUps)
                        .FirstOrDefaultAsync(r => r.RaidId == raid.RaidId);
                    if (raidToChange != null)
                    {
                        raidToChange.Title = raid.Title;
                        raidToChange.Description = raid.Description;
                        raidToChange.StartTimeUTC = raid.StartTimeUTC;
                        raidToChange.EndTimeUTC = raid.EndTimeUTC;
                        raidToChange.Organizer = raid.Organizer;
                        raidToChange.Guild = raid.Guild;
                        raidToChange.VoiceChat = raid.VoiceChat;
                        raidToChange.RaidType = raid.RaidType;
                        raidToChange.RequiredRole = raid.RequiredRole;
                        raidToChange.FreeForAllTimeUTC = raid.FreeForAllTimeUTC;
                        raidToChange.DiscordMessageId = raid.DiscordMessageId;
                        raidToChange.DiscordChannelId = raid.DiscordChannelId;
                        raidToChange.DiscordGuildId = raid.DiscordGuildId;

                        if (raidToChange.RaidType == RaidType.Planned)
                        {
                            EditRoles(raidToChange, raid, context);
                        }
                        else
                        {
                            if(!raidToChange.Roles.Where(r => r.IsRandomSignUpRole).Any())
                            {
                                raidToChange.Roles.Add(raid.Roles.FirstOrDefault(r => r.IsRandomSignUpRole));
                            }
                            int randomRoleId = raidToChange.Roles.FirstOrDefault(r => r.IsRandomSignUpRole).PlannedRaidRoleId;
                            foreach (RaidSignUp signUp in raidToChange.SignUps)
                            {
                                if (randomRoleId == 0)
                                {
                                    signUp.PlannedRaidRole = raidToChange.Roles.FirstOrDefault(r => r.IsRandomSignUpRole);
                                }
                                else
                                {
                                    signUp.PlannedRaidRoleId = randomRoleId;
                                }
                            }
                            context.PlannedRaidRoles.RemoveRange(raidToChange.Roles.Where(r => !r.IsRandomSignUpRole));
                        }

                        EditReminders(raidToChange, raid, context);
                    }

                    await context.SaveChangesAsync();
                }
            }
        }

        private void EditRoles(Raid raidToEdit, Raid raid, LiebContext context)
        {
            List<PlannedRaidRole> rolesToRemove = new List<PlannedRaidRole>();
            foreach (PlannedRaidRole role in raidToEdit.Roles)
            {
                PlannedRaidRole? newRole = raid.Roles.FirstOrDefault(r => r.PlannedRaidRoleId == role.PlannedRaidRoleId);
                if (newRole != null)
                {
                    role.Spots = newRole.Spots;
                    role.Name = newRole.Name;
                    role.Description = newRole.Description;
                }
                else
                {
                    rolesToRemove.Add(role);
                }
            }
            foreach (PlannedRaidRole role in rolesToRemove)
            {
                raidToEdit.Roles.Remove(role);
                context.PlannedRaidRoles.Remove(role);
            }
            foreach (PlannedRaidRole role in raid.Roles.Where(r => r.PlannedRaidRoleId == 0))
            {
                raidToEdit.Roles.Add(role);
            }
        }

        private void EditReminders(Raid raidToEdit, Raid raid, LiebContext context)
        {
            List<RaidReminder> reminderToRemove = new List<RaidReminder>();
            foreach (RaidReminder reminder in raidToEdit.Reminders)
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
                    reminderToRemove.Add(reminder);
                }
            }
            foreach (RaidReminder reminder in reminderToRemove)
            {
                raidToEdit.Reminders.Remove(reminder);
                context.RaidReminders.Remove(reminder);
            }
            foreach (PlannedRaidRole role in raid.Roles.Where(r => r.PlannedRaidRoleId == 0))
            {
                raidToEdit.Roles.Add(role);
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

        public async Task SignOff(int raidId, int liebUserId)
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
                if(raid != null && raid.RaidType != RaidType.Planned && !signUp.PlannedRaidRole.IsRandomSignUpRole)
                {
                    context.PlannedRaidRoles.Remove(signUp.PlannedRaidRole);
                    signUp.PlannedRaidRole = raid.Roles.FirstOrDefault(r => r.IsRandomSignUpRole);
                }
            }

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
            if (!IsRoleSignUpAllowed(raidId, liebUserId, plannedRoleId, signUpType, true))
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

        public bool IsRoleSignUpAllowed(int liebUserId, int plannedRoleId, SignUpType signUpType)
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

        public bool IsRoleSignUpAllowed(int raidId, int liebUserId, int plannedRoleId, SignUpType signUpType, bool moveFlexUser)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
            .Include(r => r.Roles)
            .Include(r => r.SignUps)
            .FirstOrDefault(r => r.RaidId == raidId);

            if (raid == null) return false;

            if (raid.RaidType == RaidType.Planned)
            {
                //if (raid.MoveFlexAllowed)
                {
                    return IsRoleSignUpAllowed(raid, liebUserId, plannedRoleId, signUpType, moveFlexUser, new List<int>()).Result;
                }
                //else
                //{
                //    return IsRoleSignUpAllowed(liebUserId, plannedRoleId, signUpType);
                //}
            }
            else
            {
                PlannedRaidRole? role = context.PlannedRaidRoles
                    .AsNoTracking()
                    .FirstOrDefault(r => r.PlannedRaidRoleId == plannedRoleId);
                if(role == null) return false;
                if (role.IsRandomSignUpRole)
                {
                    // new sign up is available if there are free spots and the user is not signed up or still in the random role
                    RaidSignUp? signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == liebUserId);
                    return raid.SignUps.Where(s => s.SignUpType == SignUpType.SignedUp).Count() < role.Spots
                        && (signUp == null || signUp.PlannedRaidRoleId == plannedRoleId || signUp.SignUpType == SignUpType.SignedOff);
                }
                return raid.SignUps.Where(s => s.LiebUserId == liebUserId && s.PlannedRaidRoleId == plannedRoleId).Any();
            }
        }

        private async Task<bool> IsRoleSignUpAllowed(Raid raid, int liebUserId, int plannedRoleId, SignUpType signUpType, bool moveFlexUser, List<int> checkedRoleIds)
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

            foreach (int userId in raid.SignUps.Where(s => s.PlannedRaidRoleId == plannedRoleId && s.SignUpType == SignUpType.SignedUp).Select(s => s.LiebUserId))
            {
                foreach (RaidSignUp signUp in raid.SignUps.Where(s => s.LiebUserId == userId && s.SignUpType == SignUpType.Flex))
                {
                    if (!checkedRoleIds.Contains(signUp.PlannedRaidRoleId)
                        && await IsRoleSignUpAllowed(raid, userId, signUp.PlannedRaidRoleId, SignUpType.SignedUp, moveFlexUser, checkedRoleIds))
                    {
                        if (moveFlexUser)
                        {
                            await ChangeSignUpType(raid.RaidId, userId, signUp.PlannedRaidRoleId, SignUpType.SignedUp);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsRaidSignUpAllowed(int liebUserId, int raidId, out string errorMessage)
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
                .FirstOrDefault(r => r.LiebUserId == liebUserId);
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
                return false;
            }

            return true;
        }

        public void SendReminders()
        {
            using var context = _contextFactory.CreateDbContext();
            List<Raid> raids = context.Raids
                .Include(r => r.Reminders)
                .Where(raid => raid.Reminders.Where(reminder => !reminder.Sent && raid.StartTimeUTC.AddHours(-reminder.HoursBeforeRaid) < DateTime.UtcNow).Any())
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
