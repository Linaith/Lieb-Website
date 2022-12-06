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
                .Include(r => r.Reminders)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.LiebUser)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.GuildWars2Account)
                .ThenInclude(a => a.EquippedBuilds)
                .ThenInclude(e => e.GuildWars2Build)
                .Include(r => r.SignUps)
                .ThenInclude(s => s.RaidRole)
                .FirstOrDefault(r => r.RaidId == raidId);

            if (raid == null || raid.RaidType == RaidType.Planned)
                return;

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
            await context.SaveChangesAsync();
            CleanUpRoles(raid, context);
            await context.SaveChangesAsync();
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
                RaidRole role = new RaidRole();
                role.Spots = 0;
                if (possibleClasses.Count > 0)
                {
                    role.Name = possibleClasses.ToList()[_random.Next(possibleClasses.Count - 1)].ToString();
                }
                else
                {
                    role.Name = "No class found.";
                }
                raid.Roles.Add(role);
                signUp.RaidRole = role;
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
                RaidRole role = new RaidRole();
                role.Spots = 0;
                if (possibleEliteSpecs.Count > 0)
                {
                    role.Name = possibleEliteSpecs.ToList()[_random.Next(possibleEliteSpecs.Count - 1)].ToString();
                }
                else
                {
                    role.Name = "No class found.";
                }
                raid.Roles.Add(role);
                signUp.RaidRole = role;
            }
        }
    
        private void RandomizeWithBoons(Raid raid)
        {
            int noGroups = (raid.SignUps.Where(s => s.SignUpType != SignUpType.Flex && s.SignUpType != SignUpType.SignedOff).Count() / 5) +1;

            List<GuildWars2Account> usedAccounts = raid.SignUps.Where(s => s.GuildWars2Account != null && s.GuildWars2Account.EquippedBuilds.Count > 0).Select(s => s.GuildWars2Account).ToList();
            Dictionary<GuildWars2Account, GuildWars2Build> addedAccounts = new Dictionary<GuildWars2Account, GuildWars2Build>();

            List<GuildWars2Account> alacHealers = usedAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType == DamageType.Heal && b.GuildWars2Build.Alacrity).Any()).ToList();
            List<GuildWars2Account> quickHealers = usedAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType == DamageType.Heal && b.GuildWars2Build.Quickness).Any()).ToList();
            List<GuildWars2Account> alacDps = usedAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.Alacrity).Any()).ToList();
            List<GuildWars2Account> quickDps = usedAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.Quickness).Any()).ToList();

            List<Tuple<int, int>> possibleBoonCombinations = new List<Tuple<int, int>>();
            foreach(GuildWars2Account alac in alacHealers)
            {
                foreach(GuildWars2Account quick in quickDps)
                {
                    if(alac.GuildWars2AccountId != quick.GuildWars2AccountId)
                        possibleBoonCombinations.Add(new Tuple<int, int>(alac.GuildWars2AccountId, quick.GuildWars2AccountId));
                }
            }
            foreach(GuildWars2Account quick in quickHealers)
            {
                foreach(GuildWars2Account alac in alacDps)
                {
                    if(quick.GuildWars2AccountId != alac.GuildWars2AccountId)
                        possibleBoonCombinations.Add(new Tuple<int, int>(quick.GuildWars2AccountId, alac.GuildWars2AccountId));
                }
            }

            possibleBoonCombinations = possibleBoonCombinations.OrderBy(u => _random.Next()).ToList();
            List<Tuple<int, int>> usedBoonCombinations = new List<Tuple<int, int>>();
            foreach(var boonCombination in possibleBoonCombinations)
            {
                if(!usedBoonCombinations.Where(b => b.Item1 == boonCombination.Item1 || b.Item1 == boonCombination.Item2
                                                   || b.Item2 == boonCombination.Item1 || b.Item2 == boonCombination.Item2).Any()
                    && usedBoonCombinations.Count < noGroups)
                {
                    usedBoonCombinations.Add(new Tuple<int, int>(boonCombination.Item1, boonCombination.Item2));
                }
            }


            //TODO: avoid the same GW2 account multiple times in a raid?
            foreach(var boonCombination in usedBoonCombinations)
            {
                GuildWars2Account healer = usedAccounts.First(a => a.GuildWars2AccountId == boonCombination.Item1);
                GuildWars2Account boonDps = usedAccounts.First(a => a.GuildWars2AccountId == boonCombination.Item2);
                Tuple<GuildWars2Build, GuildWars2Build> builds = GetBoonBuilds(healer, boonDps);
                AddRole(raid, builds.Item1, raid.SignUps.First(s => s.GuildWars2AccountId == boonCombination.Item1));
                addedAccounts.Add(healer, builds.Item1);
                AddRole(raid, builds.Item2, raid.SignUps.First(s => s.GuildWars2AccountId == boonCombination.Item2));
                addedAccounts.Add(boonDps, builds.Item2);
            }

            //add aditional healers
            if(usedBoonCombinations.Count < noGroups)
            {
                List<GuildWars2Account> additionalHealers = usedAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType == DamageType.Heal).Any()
                                                                            && !addedAccounts.Where(x => x.Key.GuildWars2AccountId == a.GuildWars2AccountId).Any()).ToList();
                additionalHealers = additionalHealers.OrderBy(u => _random.Next()).ToList();
                int neededHealers = noGroups - usedBoonCombinations.Count;
                foreach(GuildWars2Account healer in additionalHealers)
                {
                    if(neededHealers > 0)
                    {
                        GuildWars2Build build = healer.EquippedBuilds.First(b => b.GuildWars2Build.DamageType == DamageType.Heal).GuildWars2Build;
                        AddRole(raid, build, raid.SignUps.First(s => s.GuildWars2AccountId == healer.GuildWars2AccountId));
                        addedAccounts.Add(healer, build);
                        neededHealers --;
                    }
                }
            }

            //add might
            if(addedAccounts.Where(a => a.Value.Might).Count() < noGroups )
            {
                List<GuildWars2Account> additionalMight = usedAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.Might).Any()
                                                                            && !addedAccounts.Where(x => x.Key.GuildWars2AccountId == a.GuildWars2AccountId).Any()).ToList();
                additionalMight = additionalMight.OrderBy(u => _random.Next()).ToList();
                int neededMight = noGroups - addedAccounts.Where(a => a.Value.Might).Count();
                foreach(GuildWars2Account mightAccount in additionalMight)
                {
                    if(neededMight > 0)
                    {
                        GuildWars2Build build = mightAccount.EquippedBuilds.First(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.Might).GuildWars2Build;
                        AddRole(raid, build, raid.SignUps.First(s => s.GuildWars2AccountId == mightAccount.GuildWars2AccountId));
                        addedAccounts.Add(mightAccount, build);
                        neededMight --;
                    }
                }
            }

            //add dps
            List<GuildWars2Account> dpsPlayers = usedAccounts.Where(a => !addedAccounts.Where(x => x.Key.GuildWars2AccountId == a.GuildWars2AccountId).Any()).ToList();
            foreach(GuildWars2Account dps in dpsPlayers)
            {
                GuildWars2Build build = dps.EquippedBuilds.First(b => b.GuildWars2Build.DamageType != DamageType.Heal && !b.GuildWars2Build.Alacrity && !b.GuildWars2Build.Quickness).GuildWars2Build;
                AddRole(raid, build, raid.SignUps.First(s => s.GuildWars2AccountId == dps.GuildWars2AccountId));
                addedAccounts.Add(dps, build);
            }
        }

        private Tuple<GuildWars2Build, GuildWars2Build> GetBoonBuilds(GuildWars2Account healer, GuildWars2Account boonDps)
        {
            List<GuildWars2Build> healBuilds = healer.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType == DamageType.Heal && (b.GuildWars2Build.Alacrity || b.GuildWars2Build.Quickness)).Select(b => b.GuildWars2Build).ToList();
            List<GuildWars2Build> boonBuilds = boonDps.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType != DamageType.Heal && (b.GuildWars2Build.Alacrity || b.GuildWars2Build.Quickness)).Select(b => b.GuildWars2Build).ToList();
            healBuilds.OrderBy(u => _random.Next()).ToList();
            boonBuilds.OrderBy(u => _random.Next()).ToList();

            foreach(GuildWars2Build healBuild in healBuilds)
            {
                if(boonBuilds.Where(b => b.Alacrity != healBuild.Alacrity).Any())
                {
                    GuildWars2Build boonBuild = boonBuilds.First(b => b.Alacrity != healBuild.Alacrity);
                    return new Tuple<GuildWars2Build, GuildWars2Build>(healBuild, boonBuild);
                }
            }
            return new Tuple<GuildWars2Build, GuildWars2Build>(new GuildWars2Build(), new GuildWars2Build());
        }
    
        private void AddRole(Raid raid, GuildWars2Build usedBuild, RaidSignUp signUp )
        {
            RaidRole role = new RaidRole();
            role.Spots = 0;
            role.Name = usedBuild.BuildName;
            raid.Roles.Add(role);
            signUp.RaidRole = role;
        }

        private void CleanUpRoles(Raid raid, LiebContext context)
        {
            List<RaidRole> rolesToDelete = new List<RaidRole>();
            foreach (RaidRole role in raid.Roles)
            {
                if (!role.IsRandomSignUpRole && raid.SignUps.FirstOrDefault(s => s.RaidRoleId == role.RaidRoleId) == null)
                {
                    rolesToDelete.Add(role);
                }
            }
            foreach (RaidRole role in rolesToDelete)
            {
                raid.Roles.Remove(role);
            }
            context.RaidRoles.RemoveRange(rolesToDelete);
        }

    }
}
