using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Entities.FriendsAPI;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Utils
{
    public class LunkvayDBContext(DbContextOptions<LunkvayDBContext> options) : DbContext(options)
    {
        private readonly string GuidDefaultSQL = "gen_random_uuid()";
        private readonly string DateTimeDefaultSQL = "TIMEZONE('UTC', NOW())";

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Avatar> Avatars { get; set; } = null!;
        public DbSet<UserProfile> Profiles { get; set; } = null!;
        public DbSet<Friendship> Friendships { get; set; } = null!;
        public DbSet<FriendshipLabel> FriendshipLabels { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<ChatMember> ChatMembers { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

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

                entity.HasOne(a => a.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(up => up.Id).HasDefaultValueSql(GuidDefaultSQL);

                entity.HasOne(up => up.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

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

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.Property(c => c.Id).HasDefaultValueSql(GuidDefaultSQL);
                entity.Property(c => c.Type).HasConversion<string>();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql(DateTimeDefaultSQL);
                entity.Property(c => c.UpdatedAt).ValueGeneratedOnUpdate().HasDefaultValueSql(DateTimeDefaultSQL);

                entity.HasIndex(c => c.CreatorId);
                entity.HasIndex(c => c.Type);
                entity.HasIndex(c => c.UpdatedAt);

                entity.HasOne(c => c.Creator).WithMany().HasForeignKey(c => c.CreatorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.LastMessage).WithOne().HasForeignKey<Chat>(c => c.LastMessageId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ChatMember>(entity =>
            {
                entity.Property(cm => cm.Id).HasDefaultValueSql(GuidDefaultSQL);
                entity.Property(cm => cm.Role).HasConversion<string>();

                entity.HasIndex(cm => cm.ChatId);
                entity.HasIndex(cm => cm.MemberId);
                entity.HasIndex(cm => new { cm.ChatId, cm.MemberId }).IsUnique();

                entity.HasOne(cm => cm.Chat).WithMany(c => c.Members).HasForeignKey(cm => cm.ChatId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(cm => cm.Member).WithMany().HasForeignKey(cm => cm.MemberId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.Property(cm => cm.Id).HasDefaultValueSql(GuidDefaultSQL);
                entity.Property(cm => cm.SystemMessageType).HasConversion<string>();
                entity.Property(cm => cm.IsEdited).HasDefaultValue(false);
                entity.Property(cm => cm.IsPinned).HasDefaultValue(false);
                entity.Property(cm => cm.IsDeleted).HasDefaultValue(false);
                entity.Property(cm => cm.CreatedAt).HasDefaultValueSql(DateTimeDefaultSQL);
                entity.Property(cm => cm.UpdatedAt).ValueGeneratedOnUpdate().HasDefaultValueSql(DateTimeDefaultSQL);

                entity.HasIndex(cm => cm.ChatId);
                entity.HasIndex(cm => cm.SenderId);
                entity.HasIndex(cm => new { cm.SenderId, cm.CreatedAt });
                entity.HasIndex(cm => cm.IsDeleted);
                entity.HasIndex(cm => new { cm.ChatId, cm.IsPinned });
                entity.HasIndex(cm => cm.CreatedAt);

                entity.HasOne(cm => cm.Chat).WithMany(c => c.Messages).HasForeignKey(cm => cm.ChatId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(cm => cm.Sender).WithMany().HasForeignKey(cm => cm.SenderId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
