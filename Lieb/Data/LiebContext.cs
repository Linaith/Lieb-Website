#nullable disable
using Lieb.Models;
using Lieb.Models.GuildWars2;
using Lieb.Models.GuildWars2.Raid;
using Microsoft.EntityFrameworkCore;

namespace Lieb.Data
{
    public class LiebContext : DbContext
    {
        public LiebContext (DbContextOptions<LiebContext> options)
            : base(options)
        {
        }

        public DbSet<LiebUser> LiebUsers { get; set; }
        public DbSet<RoleAssignment> RoleAssignments { get; set; }
        public DbSet<LiebRole> LiebRoles { get; set; }
        public DbSet<GuildWars2Account> GuildWars2Accounts { get; set; }
        public DbSet<Equipped> Equipped { get; set; }
        public DbSet<GuildWars2Build> GuildWars2Builds { get; set; }
        public DbSet<RaidRole> RaidRoles { get; set; }
        public DbSet<Raid> Raids { get; set; }
        public DbSet<RaidTemplate> RaidTemplates { get; set; }
        public DbSet<RaidReminder> RaidReminders { get; set; }
        public DbSet<RaidSignUp> RaidSignUps { get; set; }
        public DbSet<RaidSignUpHistory> RaidSignUpHistories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LiebUser>().ToTable("LiebUser");
            modelBuilder.Entity<RoleAssignment>().ToTable("RoleAssignment");
            modelBuilder.Entity<LiebRole>().ToTable("LiebRole");
            modelBuilder.Entity<GuildWars2Account>().ToTable("GuildWars2Account");
            modelBuilder.Entity<Equipped>().ToTable("Equipped");
            modelBuilder.Entity<GuildWars2Build>().ToTable("GuildWars2Build");
            modelBuilder.Entity<RaidRole>().ToTable("RaidRole");
            modelBuilder.Entity<Raid>().ToTable("Raid");
            modelBuilder.Entity<RaidTemplate>().ToTable("RaidTemplate");
            modelBuilder.Entity<RaidReminder>().ToTable("RaidReminder");
            modelBuilder.Entity<RaidSignUp>().ToTable("RaidSignUp");
            modelBuilder.Entity<RaidSignUpHistory>().ToTable("RaidSignUpHistory");
        }
    }
}
