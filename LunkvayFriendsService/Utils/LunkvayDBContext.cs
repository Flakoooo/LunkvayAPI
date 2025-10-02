using LunkvayFriendsService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunkvayFriendsService.Utils
{
    public class LunkvayDBContext(DbContextOptions<LunkvayDBContext> options) : DbContext(options)
    {
        private readonly string GuidDefaultSQL = "gen_random_uuid()";
        private readonly string DateTimeDefaultSQL = "TIMEZONE('UTC', NOW())";

        public DbSet<Friendship> Friendships { get; set; } = null!;
        public DbSet<FriendshipLabel> FriendshipLabels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.Property(f => f.Id).HasDefaultValueSql(GuidDefaultSQL);
                entity.Property(f => f.CreatedAt).HasDefaultValueSql(DateTimeDefaultSQL);
                entity.Property(f => f.UpdatedAt).ValueGeneratedOnUpdate().HasDefaultValueSql(DateTimeDefaultSQL);

                entity.HasIndex(f => new { f.UserId1, f.UserId2 }).IsUnique();
                entity.HasIndex(f => f.Status);

                entity.HasOne(f => f.User1).WithMany().HasForeignKey(f => f.UserId1).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(f => f.User2).WithMany().HasForeignKey(f => f.UserId2).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(f => f.Initiator).WithMany().HasForeignKey(f => f.InitiatorId);
            });

            modelBuilder.Entity<FriendshipLabel>(entity =>
            {
                entity.Property(fl => fl.Id).HasDefaultValueSql(GuidDefaultSQL);

                entity.HasIndex(fl => fl.FriendshipId);
                entity.HasIndex(fl => fl.CreatorId);

                entity.HasOne(fl => fl.Friendship).WithMany(f => f.Labels).HasForeignKey(fl => fl.FriendshipId);
                entity.HasOne(fl => fl.Creator).WithMany().HasForeignKey(fl => fl.CreatorId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
