using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Entities.FriendsAPI;
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
            _ = modelBuilder.Entity<User>(static entity =>
            {
                _ = entity.Property(static u => u.Id).HasDefaultValueSql("gen_random_uuid()");
                _ = entity.Property(static u => u.CreatedAt).HasDefaultValueSql("TIMEZONE('UTC', NOW())");
                _ = entity.Property(static u => u.LastLogin).HasDefaultValueSql("TIMEZONE('UTC', NOW())");
                _ = entity.HasIndex(static u => u.IsDeleted);
            });

            _ = modelBuilder.Entity<Avatar>(static entity =>
            {
                _ = entity.Property(static a => a.Id).HasDefaultValueSql("gen_random_uuid()");
                _ = entity.Property(static u => u.UpdatedAt).HasDefaultValueSql("TIMEZONE('UTC', NOW())");

                _ = entity.HasOne(static a => a.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<UserProfile>(static entity =>
            {
                _ = entity.Property(static up => up.Id).HasDefaultValueSql("gen_random_uuid()");

                _ = entity.HasOne(static up => up.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<Friendship>(static entity =>
            {
                _ = entity.Property(static f => f.Id).HasDefaultValueSql("gen_random_uuid()");
                _ = entity.HasIndex(static f => new { f.UserId1, f.UserId2 }).IsUnique();
                _ = entity.HasIndex(static f => f.Status);
                _ = entity.Property(static f => f.CreatedAt).HasDefaultValueSql("TIMEZONE('UTC', NOW())");
                _ = entity.Property(static f => f.UpdatedAt).ValueGeneratedOnUpdate().HasDefaultValueSql("TIMEZONE('UTC', NOW())");

                _ = entity.HasOne(static f => f.User1).WithMany().HasForeignKey(static f => f.UserId1).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(static f => f.User2).WithMany().HasForeignKey(static f => f.UserId2).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(static f => f.Initiator).WithMany().HasForeignKey(static f => f.InitiatorId);
            });

            _ = modelBuilder.Entity<FriendshipLabel>(static entity =>
            {
                _ = entity.Property(static fl => fl.Id).HasDefaultValueSql("gen_random_uuid()");
                _ = entity.HasOne(static fl => fl.Friendship).WithMany(static f => f.Labels).HasForeignKey(static fl => fl.FriendshipId);
                _ = entity.HasIndex(static fl => fl.FriendshipId);
                _ = entity.HasOne(static fl => fl.Creator).WithMany().HasForeignKey(static fl => fl.CreatorId).OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasIndex(static fl => fl.CreatorId);
            });
        }
    }
}
