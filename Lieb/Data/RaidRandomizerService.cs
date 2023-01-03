using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class RaidRandomizerService
    {
        private readonly IDbContextFactory<LiebContext> _contextFactory;
        private readonly DiscordService _discordService;
        private static Random _random = new Random();

        public RaidRandomizerService(IDbContextFactory<LiebContext> contextFactory, DiscordService discordService)
        {
            _contextFactory = contextFactory;
            _discordService = discordService;
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
            await _discordService.PostRaidMessage(raidId);
        }

        private void RandomizeClasses(Raid raid)
        {
            foreach (RaidSignUp signUp in raid.SignUps.Where(s => s.RaidRole.IsRandomSignUpRole && s.SignUpType == SignUpType.SignedUp))
            {
                HashSet<GuildWars2Class> possibleClasses = new HashSet<GuildWars2Class>();
                foreach (Equipped build in signUp.GuildWars2Account.EquippedBuilds.Where(b => b.GuildWars2Build.UseInRandomRaid))
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
            foreach (RaidSignUp signUp in raid.SignUps.Where(s => s.RaidRole.IsRandomSignUpRole && s.SignUpType == SignUpType.SignedUp))
            {
                HashSet<EliteSpecialization> possibleEliteSpecs = new HashSet<EliteSpecialization>();
                foreach (Equipped build in signUp.GuildWars2Account.EquippedBuilds.Where(b => b.GuildWars2Build.UseInRandomRaid))
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
            //reset signUps to move Maybes and Backups back to Random
            RaidRole randomRole = raid.Roles.FirstOrDefault(r => r.IsRandomSignUpRole);
            foreach (RaidSignUp signUp in raid.SignUps)
            {
                signUp.RaidRole = randomRole;
            }

            int noGroups = (int)Math.Ceiling(raid.SignUps.Where(s => s.SignUpType == SignUpType.SignedUp).Count() / 5.0);

            HashSet<GuildWars2Account> signedUpAccounts = raid.SignUps.Where(s => s.GuildWars2Account != null && s.GuildWars2Account.EquippedBuilds.Where(b => b.GuildWars2Build.UseInRandomRaid).Count() > 0
                                                                        && s.SignUpType == SignUpType.SignedUp)
                                                                        .Select(s => s.GuildWars2Account).ToHashSet();
            
            List<Tuple<int, int>> possibleBoonCombinations = GetPossibleBoonCombinations(signedUpAccounts);

            List<Tuple<int, int>> usedBoonCombinations = ChooseCombinations(possibleBoonCombinations, noGroups);

            Dictionary<GuildWars2Account, GuildWars2Build> randomizedAccounts = CreateBoonRoles(raid, signedUpAccounts, usedBoonCombinations);

            if(usedBoonCombinations.Count < noGroups)
            {
                int neededHealers = noGroups - usedBoonCombinations.Count;
                AddHealers(raid, signedUpAccounts, randomizedAccounts, neededHealers);
            }

            if(randomizedAccounts.Where(a => a.Value.Might).Count() < noGroups )
            {
                AddMight(raid, signedUpAccounts, randomizedAccounts, noGroups);
            }

            AddDps(raid, signedUpAccounts, randomizedAccounts);
        }

        private List<Tuple<int, int>> GetPossibleBoonCombinations(HashSet<GuildWars2Account> signedUpAccounts)
        {
            List<GuildWars2Account> alacHealers = signedUpAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType == DamageType.Heal && b.GuildWars2Build.Alacrity && b.GuildWars2Build.UseInRandomRaid).Any()).ToList();
            List<GuildWars2Account> quickHealers = signedUpAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType == DamageType.Heal && b.GuildWars2Build.Quickness && b.GuildWars2Build.UseInRandomRaid).Any()).ToList();
            List<GuildWars2Account> alacDps = signedUpAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.Alacrity && b.GuildWars2Build.UseInRandomRaid).Any()).ToList();
            List<GuildWars2Account> quickDps = signedUpAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.Quickness && b.GuildWars2Build.UseInRandomRaid).Any()).ToList();

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
            return possibleBoonCombinations;
        }

        private List<Tuple<int, int>> ChooseCombinations(List<Tuple<int, int>> possibleBoonCombinations, int noGroups)
        {
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
            return usedBoonCombinations;
        }

        private Dictionary<GuildWars2Account, GuildWars2Build> CreateBoonRoles(Raid raid, HashSet<GuildWars2Account> signedUpAccounts, List<Tuple<int, int>> usedBoonCombinations)
        {
            Dictionary<GuildWars2Account, GuildWars2Build> randomizedAccounts = new Dictionary<GuildWars2Account, GuildWars2Build>();
            foreach(var boonCombination in usedBoonCombinations)
            {
                GuildWars2Account healer = signedUpAccounts.First(a => a.GuildWars2AccountId == boonCombination.Item1);
                GuildWars2Account boonDps = signedUpAccounts.First(a => a.GuildWars2AccountId == boonCombination.Item2);
                Tuple<GuildWars2Build, GuildWars2Build> builds = GetBoonBuilds(healer, boonDps);
                AddRole(raid, builds.Item1, raid.SignUps.First(s => s.GuildWars2AccountId == boonCombination.Item1));
                randomizedAccounts.Add(healer, builds.Item1);
                AddRole(raid, builds.Item2, raid.SignUps.First(s => s.GuildWars2AccountId == boonCombination.Item2));
                randomizedAccounts.Add(boonDps, builds.Item2);
            }
            return randomizedAccounts;
        }

        private Tuple<GuildWars2Build, GuildWars2Build> GetBoonBuilds(GuildWars2Account healer, GuildWars2Account boonDps)
        {
            List<GuildWars2Build> healBuilds = healer.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType == DamageType.Heal && (b.GuildWars2Build.Alacrity || b.GuildWars2Build.Quickness) && b.GuildWars2Build.UseInRandomRaid).Select(b => b.GuildWars2Build).ToList();
            List<GuildWars2Build> boonBuilds = boonDps.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType != DamageType.Heal && (b.GuildWars2Build.Alacrity || b.GuildWars2Build.Quickness) && b.GuildWars2Build.UseInRandomRaid).Select(b => b.GuildWars2Build).ToList();
            healBuilds = healBuilds.OrderBy(u => _random.Next()).ToList();
            boonBuilds = boonBuilds.OrderBy(u => _random.Next()).ToList();

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

        private void AddHealers(Raid raid, HashSet<GuildWars2Account> signedUpAccounts, Dictionary<GuildWars2Account, GuildWars2Build> randomizedAccounts, int neededHealers)
        {
            List<GuildWars2Account> additionalHealers = signedUpAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType == DamageType.Heal && b.GuildWars2Build.UseInRandomRaid).Any()
                                                                        && !randomizedAccounts.Where(x => x.Key.GuildWars2AccountId == a.GuildWars2AccountId).Any()).ToList();
            additionalHealers = additionalHealers.OrderBy(u => _random.Next()).ToList();
            foreach(GuildWars2Account healer in additionalHealers)
            {
                if(neededHealers > 0)
                {
                    GuildWars2Build build = healer.EquippedBuilds.OrderBy(u => _random.Next()).First(b => b.GuildWars2Build.DamageType == DamageType.Heal  && b.GuildWars2Build.UseInRandomRaid).GuildWars2Build;
                    AddRole(raid, build, raid.SignUps.First(s => s.GuildWars2AccountId == healer.GuildWars2AccountId));
                    randomizedAccounts.Add(healer, build);
                    neededHealers --;
                }
            }
        }

        private void AddMight(Raid raid, HashSet<GuildWars2Account> signedUpAccounts, Dictionary<GuildWars2Account, GuildWars2Build> randomizedAccounts, int noGroups)
        {
            List<GuildWars2Account> might = signedUpAccounts.Where(a => a.EquippedBuilds.Where(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.Might && b.GuildWars2Build.UseInRandomRaid).Any()
                                                                            && !randomizedAccounts.Where(x => x.Key.GuildWars2AccountId == a.GuildWars2AccountId).Any()).ToList();
            might = might.OrderBy(u => _random.Next()).ToList();
            int neededMight = noGroups - randomizedAccounts.Where(a => a.Value.Might).Count();
            foreach(GuildWars2Account mightAccount in might)
            {
                if(neededMight > 0)
                {
                    GuildWars2Build build = mightAccount.EquippedBuilds.OrderBy(u => _random.Next()).First(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.Might && b.GuildWars2Build.UseInRandomRaid).GuildWars2Build;
                    AddRole(raid, build, raid.SignUps.First(s => s.GuildWars2AccountId == mightAccount.GuildWars2AccountId));
                    randomizedAccounts.Add(mightAccount, build);
                    neededMight --;
                }
            }
        }

        private void AddDps(Raid raid, HashSet<GuildWars2Account> signedUpAccounts, Dictionary<GuildWars2Account, GuildWars2Build> randomizedAccounts)
        {
            List<GuildWars2Account> dpsPlayers = signedUpAccounts.Where(a => !randomizedAccounts.Where(x => x.Key.GuildWars2AccountId == a.GuildWars2AccountId).Any()).ToList();
            foreach(GuildWars2Account dps in dpsPlayers)
            {
                GuildWars2Build build = dps.EquippedBuilds.OrderBy(u => _random.Next()).FirstOrDefault(b => b.GuildWars2Build.DamageType != DamageType.Heal && !b.GuildWars2Build.Alacrity && !b.GuildWars2Build.Quickness && b.GuildWars2Build.UseInRandomRaid).GuildWars2Build;
                if(build == null)
                {
                    build = dps.EquippedBuilds.OrderBy(u => _random.Next()).FirstOrDefault(b => b.GuildWars2Build.DamageType != DamageType.Heal && b.GuildWars2Build.UseInRandomRaid).GuildWars2Build;
                }
                if(build == null)
                {
                    build = dps.EquippedBuilds.OrderBy(u => _random.Next()).FirstOrDefault(b => b.GuildWars2Build.UseInRandomRaid).GuildWars2Build;
                }
                AddRole(raid, build, raid.SignUps.First(s => s.GuildWars2AccountId == dps.GuildWars2AccountId));
                randomizedAccounts.Add(dps, build);
            }
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
