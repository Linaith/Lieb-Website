using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class RaidRandomizerService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;

        public RaidRandomizerService(IDbContextFactory<LiebContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task RandomizeRaid(int raidId)
        {
            using var context = _contextFactory.CreateDbContext();
            Raid? raid = context.Raids
                .Include(r => r.Roles)
                .Include(r => r.SignUpHistory)
                .Include(r => r.Reminders)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.LiebUser)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.GuildWars2Account)
                .ThenInclude(a => a.EquippedBuilds)
                .ThenInclude(e => e.GuildWars2Build)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.PlannedRaidRole)
                .FirstOrDefault(r => r.RaidId == raidId);

            if (raid == null || raid.RaidType == RaidType.Planned)
                return;


            if (!raid.IsRandomized)
            {
                switch (raid.RaidType)
                {
                    case RaidType.RandomClasses:
                        RandomizeClasses(raid);
                        break;
                    case RaidType.RandomEliteSpecialization:
                        RandomizeEliteSpecs(raid);
                        break;
                    case RaidType.RandomWithBoons:

                        break;
                }
                raid.IsRandomized = true;
                await context.SaveChangesAsync();
                CleanUpRoles(raid, context);
                await context.SaveChangesAsync();
            }
        }

        private void RandomizeClasses(Raid raid)
        {
            Random rand = new Random();
            foreach (RaidSignUp signUp in raid.SignUps)
            {
                HashSet<GuildWars2Class> possibleClasses = new HashSet<GuildWars2Class>();
                foreach (Equipped build in signUp.GuildWars2Account.EquippedBuilds)
                {
                    possibleClasses.Add(build.GuildWars2Build.Class);
                }
                PlannedRaidRole role = new PlannedRaidRole();
                role.Spots = 1;
                if (possibleClasses.Count > 0)
                {
                    role.Name = possibleClasses.ToList()[rand.Next(possibleClasses.Count - 1)].ToString();
                }
                else
                {
                    role.Name = "No class found.";
                }
                raid.Roles.Add(role);
                signUp.PlannedRaidRole = role;
            }
        }

        private void RandomizeEliteSpecs(Raid raid)
        {
            Random rand = new Random();
            foreach (RaidSignUp signUp in raid.SignUps)
            {
                HashSet<EliteSpecialization> possibleEliteSpecs = new HashSet<EliteSpecialization>();
                foreach (Equipped build in signUp.GuildWars2Account.EquippedBuilds)
                {
                    possibleEliteSpecs.Add(build.GuildWars2Build.EliteSpecialization);
                }
                PlannedRaidRole role = new PlannedRaidRole();
                role.Spots = 1;
                if (possibleEliteSpecs.Count > 0)
                {
                    role.Name = possibleEliteSpecs.ToList()[rand.Next(possibleEliteSpecs.Count - 1)].ToString();
                }
                else
                {
                    role.Name = "No class found.";
                }
                raid.Roles.Add(role);
                signUp.PlannedRaidRole = role;
            }
        }

        private void CleanUpRoles(Raid raid, LiebContext context)
        {
            List<PlannedRaidRole> rolesToDelete = new List<PlannedRaidRole>();
            foreach (PlannedRaidRole role in raid.Roles)
            {
                if (raid.SignUps.FirstOrDefault(s => s.PlannedRaidRoleId == role.PlannedRaidRoleId) == null)
                {
                    rolesToDelete.Add(role);
                }
            }
            foreach (PlannedRaidRole role in rolesToDelete)
            {
                raid.Roles.Remove(role);
            }
            context.PlannedRaidRoles.RemoveRange(rolesToDelete);
        }

    }
}
