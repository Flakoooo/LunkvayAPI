using LunkvayAPI.src.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Utils
{
    public class LunkvayDBContext(DbContextOptions<LunkvayDBContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Avatar> Avatars { get; set; } = null!;
        public DbSet<UserProfile> Profiles { get; set; } = null!;
        public DbSet<Friendship> Friendships { get; set; } = null!;
        public DbSet<FriendshipLabel> FriendshipLabels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("TIMEZONE('UTC', NOW())");
                entity.Property(u => u.LastLogin).HasDefaultValueSql("TIMEZONE('UTC', NOW())");
                entity.HasIndex(u => u.IsDeleted);
            });

            modelBuilder.Entity<Avatar>(entity =>
            {
                entity.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("TIMEZONE('UTC', NOW())");

                entity.HasOne(a => a.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(up => up.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.HasOne(up => up.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.Property(f => f.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.HasIndex(f => new { f.UserId1, f.UserId2 }).IsUnique();
                entity.HasIndex(f => f.Status);
                entity.Property(f => f.CreatedAt).HasDefaultValueSql("TIMEZONE('UTC', NOW())");
                entity.Property(f => f.UpdatedAt).ValueGeneratedOnUpdate().HasDefaultValueSql("TIMEZONE('UTC', NOW())");

                entity.HasOne(f => f.User1).WithMany().HasForeignKey(f => f.UserId1).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(f => f.User2).WithMany().HasForeignKey(f => f.UserId2).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(f => f.Initiator).WithMany().HasForeignKey(f => f.InitiatorId);
            });

            modelBuilder.Entity<FriendshipLabel>(entity =>
            {
                entity.Property(fl => fl.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.HasOne(fl => fl.Friendship).WithMany(f => f.Labels).HasForeignKey(fl => fl.FriendshipId);
                entity.HasIndex(fl => fl.FriendshipId);
                entity.HasOne(fl => fl.Creator).WithMany().HasForeignKey(fl => fl.CreatorId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(fl => fl.CreatorId);
            });
        }
    }
}
