using LunkvayIdentityService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunkvayIdentityService.Utils
{
    public class LunkvayDBContext(DbContextOptions<LunkvayDBContext> options) : DbContext(options)
    {
        private readonly string GuidDefaultSQL = "gen_random_uuid()";
        private readonly string DateTimeDefaultSQL = "TIMEZONE('UTC', NOW())";

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Avatar> Avatars { get; set; } = null!;
        public DbSet<UserProfile> Profiles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasDefaultValueSql(GuidDefaultSQL);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql(DateTimeDefaultSQL);
                entity.Property(u => u.LastLogin).HasDefaultValueSql(DateTimeDefaultSQL);

                entity.HasIndex(u => u.IsDeleted);
            });

            modelBuilder.Entity<Avatar>(entity =>
            {
                entity.Property(a => a.Id).HasDefaultValueSql(GuidDefaultSQL);
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql(DateTimeDefaultSQL);

                entity.HasIndex(a => a.UserId);

                entity.HasOne(a => a.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(up => up.Id).HasDefaultValueSql(GuidDefaultSQL);

                entity.HasOne(up => up.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
