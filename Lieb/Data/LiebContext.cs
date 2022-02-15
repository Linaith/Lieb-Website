#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lieb.Models;
using Lieb.Models.Raid;

namespace Lieb.Data
{
    public class LiebContext : DbContext
    {
        public LiebContext (DbContextOptions<LiebContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<GuildWars2Account> GuildWars2Account { get; set; }
        public DbSet<Equipped> Equipped { get; set; }
        public DbSet<RaidRole> RaidRoles { get; set; }
        public DbSet<PlannedRaid> PlannedRaids { get; set; }
        public DbSet<PlannedRaidRole> PlannedRaidRoles { get; set; }
        public DbSet<Raid> Raids { get; set; }
        public DbSet<RaidReminder> RaidReminders { get; set; }
        public DbSet<RaidSignUp> RaidSignUps { get; set; }
        public DbSet<RandomRaid> RandomRaids { get; set; }
        public DbSet<SignUpHistory> SignUpHistories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<GuildWars2Account>().ToTable("GuildWars2Account");
            modelBuilder.Entity<Equipped>().ToTable("Equipped");
            modelBuilder.Entity<RaidRole>().ToTable("RaidRole");
            modelBuilder.Entity<PlannedRaid>().ToTable("PlannedRaid");
            modelBuilder.Entity<PlannedRaidRole>().ToTable("PlannedRaidRole");
            modelBuilder.Entity<Raid>().ToTable("Raid");
            modelBuilder.Entity<RaidReminder>().ToTable("RaidReminder");
            modelBuilder.Entity<RaidSignUp>().ToTable("RaidSignUp");
            modelBuilder.Entity<RandomRaid>().ToTable("RandomRaid");
            modelBuilder.Entity<SignUpHistory>().ToTable("SignUpHistory");
        }
    }
}
