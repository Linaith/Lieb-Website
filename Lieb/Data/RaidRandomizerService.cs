using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class RaidRandomizerService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private static Random _random = new Random();

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
                        RandomizeWithBoons(raid);
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
                    role.Name = possibleClasses.ToList()[_random.Next(possibleClasses.Count - 1)].ToString();
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
                    role.Name = possibleEliteSpecs.ToList()[_random.Next(possibleEliteSpecs.Count - 1)].ToString();
                }
                else
                {
                    role.Name = "No class found.";
                }
                raid.Roles.Add(role);
                signUp.PlannedRaidRole = role;
            }
        }

        private void RandomizeWithBoons(Raid raid)
        {
            Dictionary<int, GuildWars2Build> signedUpUsers= new Dictionary<int, GuildWars2Build>();
            foreach (RaidSignUp signUp in raid.SignUps)
            {
                signedUpUsers.Add(signUp.LiebUserId, signUp.GuildWars2Account.EquippedBuilds.ToList()[_random.Next(signUp.GuildWars2Account.EquippedBuilds.Count - 1)].GuildWars2Build);
            }
            BalanceRoles(raid, signedUpUsers);
            foreach(var userBuild in signedUpUsers)
            {
                PlannedRaidRole role = new PlannedRaidRole();
                role.Spots = 1;
                role.Name = userBuild.Value.BuildName;
                raid.Roles.Add(role);
                RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                signUp.PlannedRaidRole = role;
            }
        }

        private void BalanceRoles(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int recusrionDepth = 0)
        {
            int Alac = 0;
            int Quick = 0;
            int Heal = 0;
            int Might = 0;

            signedUpUsers = signedUpUsers.OrderBy(u => _random.Next()).ToDictionary(u => u.Key, u => u.Value);

            foreach(GuildWars2Build build in signedUpUsers.Values)
            {
                Alac += build.Alacrity ? 5 : 0;
                Quick += build.Quickness ? 5 : 0;
                Heal += build.Heal ? 5 : 0;
                Might += build.Might ? 5 : 0;
            }
            if(Alac > 10)
            {
                ReduceAlac(raid, signedUpUsers, Alac);
            }
            if(Alac < 10)
            {
                IncreaseAlac(raid, signedUpUsers, Alac);
            }
            signedUpUsers = signedUpUsers.OrderBy(u => _random.Next()).ToDictionary(u => u.Key, u => u.Value);
            if (Quick > 10)
            {
                ReduceQuick(raid, signedUpUsers, Quick);
            }
            if (Quick < 10)
            {
                IncreaseQuick(raid, signedUpUsers, Quick);
            }
            signedUpUsers = signedUpUsers.OrderBy(u => _random.Next()).ToDictionary(u => u.Key, u => u.Value);
            if (Heal > 10)
            {
                ReduceHeal(raid, signedUpUsers, Heal);
            }
            if (Heal < 10)
            {
                IncreaseHeal(raid, signedUpUsers, Heal);
            }
            signedUpUsers = signedUpUsers.OrderBy(u => _random.Next()).ToDictionary(u => u.Key, u => u.Value);
            if (Might > 10)
            {
                ReduceMight(raid, signedUpUsers, Might);
            }
            if (Might < 10)
            {
                IncreaseMight(raid, signedUpUsers, Might);
            }

            if(recusrionDepth < 20)
            {
                recusrionDepth++;
                BalanceRoles(raid, signedUpUsers, recusrionDepth);
            }
        }

        private void ReduceAlac(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int currentAlac)
        {
            foreach (var userBuild in signedUpUsers)
            {
                if(userBuild.Value.Alacrity && currentAlac > 10)
                {
                    RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                    Equipped newBuild = signUp.GuildWars2Account.EquippedBuilds.Where(b => !b.GuildWars2Build.Alacrity).OrderBy(u => _random.Next()).FirstOrDefault();
                    if (newBuild != null)
                    {
                        signedUpUsers[userBuild.Key] = newBuild.GuildWars2Build;
                        currentAlac -= 5;
                    }
                }
            }
        }

        private void IncreaseAlac(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int currentAlac)
        {
            foreach (var userBuild in signedUpUsers)
            {
                if (!userBuild.Value.Alacrity && currentAlac < 10)
                {
                    RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                    Equipped newBuild = signUp.GuildWars2Account.EquippedBuilds.Where(b => b.GuildWars2Build.Alacrity).OrderBy(u => _random.Next()).FirstOrDefault();
                    if (newBuild != null)
                    {
                        signedUpUsers[userBuild.Key] = newBuild.GuildWars2Build;
                        currentAlac += 5;
                    }
                }
            }
        }

        private void ReduceQuick(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int currentQuick)
        {
            foreach (var userBuild in signedUpUsers)
            {
                if (userBuild.Value.Quickness && currentQuick > 10)
                {
                    RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                    Equipped newBuild = signUp.GuildWars2Account.EquippedBuilds.Where(b => !b.GuildWars2Build.Quickness).OrderBy(u => _random.Next()).FirstOrDefault();
                    if (newBuild != null)
                    {
                        signedUpUsers[userBuild.Key] = newBuild.GuildWars2Build;
                        currentQuick -=5;
                    }
                }
            }
        }

        private void IncreaseQuick(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int currentQuick)
        {
            foreach (var userBuild in signedUpUsers)
            {
                if (!userBuild.Value.Quickness && currentQuick < 10)
                {
                    RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                    Equipped newBuild = signUp.GuildWars2Account.EquippedBuilds.Where(b => b.GuildWars2Build.Quickness).OrderBy(u => _random.Next()).FirstOrDefault();
                    if (newBuild != null)
                    {
                        signedUpUsers[userBuild.Key] = newBuild.GuildWars2Build;
                        currentQuick += 5;
                    }
                }
            }
        }

        private void ReduceMight(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int currentMight)
        {
            foreach (var userBuild in signedUpUsers)
            {
                if (userBuild.Value.Might && currentMight > 10)
                {
                    RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                    Equipped newBuild = signUp.GuildWars2Account.EquippedBuilds.Where(b => !b.GuildWars2Build.Might).OrderBy(u => _random.Next()).FirstOrDefault();
                    if (newBuild != null)
                    {
                        signedUpUsers[userBuild.Key] = newBuild.GuildWars2Build;
                        currentMight -=5;
                    }
                }
            }
        }

        private void IncreaseMight(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int currentMight)
        {
            foreach (var userBuild in signedUpUsers)
            {
                if (!userBuild.Value.Might && currentMight < 10)
                {
                    RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                    Equipped newBuild = signUp.GuildWars2Account.EquippedBuilds.Where(b => b.GuildWars2Build.Might).OrderBy(u => _random.Next()).FirstOrDefault();
                    if (newBuild != null)
                    {
                        signedUpUsers[userBuild.Key] = newBuild.GuildWars2Build;
                        currentMight += 5;
                    }
                }
            }
        }

        private void ReduceHeal(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int currentHeal)
        {
            foreach (var userBuild in signedUpUsers)
            {
                if (userBuild.Value.Heal && currentHeal > 10)
                {
                    RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                    Equipped newBuild = signUp.GuildWars2Account.EquippedBuilds.Where(b => !b.GuildWars2Build.Heal).OrderBy(u => _random.Next()).FirstOrDefault();
                    if (newBuild != null)
                    {
                        signedUpUsers[userBuild.Key] = newBuild.GuildWars2Build;
                        currentHeal -= 5;
                    }
                }
            }
        }

        private void IncreaseHeal(Raid raid, Dictionary<int, GuildWars2Build> signedUpUsers, int currentHeal)
        {
            foreach (var userBuild in signedUpUsers)
            {
                if (!userBuild.Value.Heal && currentHeal < 10)
                {
                    RaidSignUp signUp = raid.SignUps.FirstOrDefault(s => s.LiebUserId == userBuild.Key);
                    Equipped newBuild = signUp.GuildWars2Account.EquippedBuilds.Where(b => b.GuildWars2Build.Heal).OrderBy(u => _random.Next()).FirstOrDefault();
                    if (newBuild != null)
                    {
                        signedUpUsers[userBuild.Key] = newBuild.GuildWars2Build;
                        currentHeal += 5;
                    }
                }
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
