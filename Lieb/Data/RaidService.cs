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

        public async Task CreateRaid(Raid raid)
        {
            if (raid == null)
            {
                using var context = _contextFactory.CreateDbContext();
                context.Raids.Add(raid);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<PlannedRaidRole>> GetFreeRoles(int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = await context.Raids
                .Include(r => r.Roles)
                .Include(r => r.SignUps)
                .FirstOrDefaultAsync(r => r.RaidId == raidId);

            List<PlannedRaidRole> freeRoles = new List<PlannedRaidRole>();
            if (raid != null)
            {
                List<int> plannedRolesIds = raid.SignUps.Select(s => s.PlannedRaidRoleId).ToList();
                Dictionary<int, int> addedIds = new Dictionary<int, int>();

                foreach (RaidSignUp signUp in raid.SignUps)
                {
                    if (signUp.SignUpType == SignUpType.SignedUp)
                    {
                        int id = signUp.PlannedRaidRoleId;
                        if (addedIds.ContainsKey(id))
                        {
                            addedIds[id] += 1;
                        }
                        else
                        {
                            addedIds.Add(id, 1);
                        }
                    }
                }
                foreach(PlannedRaidRole role in raid.Roles)
                {
                    if(!addedIds.ContainsKey(role.PlannedRaidRoleId) || role.Spots > addedIds[role.PlannedRaidRoleId])
                    {
                        freeRoles.Add(role);
                    }
                }
            }
            return freeRoles;
        }

        public async Task SignUp(int raidId, int liebUserId, int guildWars2AccountId, int plannedRoleId, SignUpType signUpType)
        {
            if (!IsSignUpAllowed(liebUserId, plannedRoleId, signUpType))
            {
                return;
            }
            using var context = _contextFactory.CreateDbContext();
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

        public async Task SignOff(int raidId, int liebUserId)
        {
            using var context = _contextFactory.CreateDbContext();
            List<RaidSignUp> signUps = context.RaidSignUps.Where(x => x.RaidId == raidId && x.LiebUserId == liebUserId).ToList();
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

            RaidSignUp signUp = await context.RaidSignUps.FirstOrDefaultAsync(x => x.RaidId == raidId && x.LiebUserId == liebUserId && x.SignUpType != SignUpType.SignedOff && x.SignUpType != SignUpType.Flex);
            signUp.PlannedRaidRoleId = plannedRoleId;
            signUp.SignUpType = signUpType;
            await context.SaveChangesAsync();
        }

        public bool IsSignUpAllowed(int liebUserId, int plannedRoleId, SignUpType signUpType)
        {
            if(signUpType == SignUpType.Backup || signUpType == SignUpType.Flex || signUpType == SignUpType.SignedOff)
            {
                return true;
            }

            using var context = _contextFactory.CreateDbContext();

            List<RaidSignUp> signUps = context.RaidSignUps.Where(s => s.PlannedRaidRoleId == plannedRoleId).ToList();

            PlannedRaidRole role = context.PlannedRaidRoles
            .FirstOrDefault(r => r.PlannedRaidRoleId == plannedRoleId);

            if (role == null)
                return false;

            if(signUps.Count(s => s.SignUpType == SignUpType.SignedUp) < role.Spots)
                return true;

            return signUps.Where(s => s.LiebUserId == liebUserId && s.SignUpType == SignUpType.SignedUp).Any();
        }
    }
}
