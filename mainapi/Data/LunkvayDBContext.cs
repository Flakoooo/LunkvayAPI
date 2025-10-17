using LunkvayAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Data
{
    public class LunkvayDBContext(DbContextOptions<LunkvayDBContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Avatar> Avatars { get; set; } = null!;
        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<Friendship> Friendships { get; set; } = null!;
        public DbSet<FriendshipLabel> FriendshipLabels { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<ChatImage> ChatImages { get; set; } = null!;
        public DbSet<ChatMember> ChatMembers { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("char(36)").IsRequired();
                entity.Property(u => u.CreatedAt).IsRequired();
                entity.Property(u => u.LastLogin).IsRequired();

                entity.HasIndex(u => u.IsDeleted);
            });

            modelBuilder.Entity<Avatar>(entity =>
            {
                entity.Property(a => a.Id).HasColumnType("char(36)").IsRequired();
                entity.Property(u => u.UpdatedAt).IsRequired();

                entity.HasIndex(a => a.UserId);

                entity.HasOne(a => a.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.Property(up => up.Id).HasColumnType("char(36)").IsRequired();

                entity.HasOne(up => up.User).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.Property(f => f.Id).HasColumnType("char(36)").IsRequired();
                entity.Property(f => f.CreatedAt).IsRequired();

                entity.HasIndex(f => new { f.UserId1, f.UserId2 }).IsUnique();
                entity.HasIndex(f => f.Status);

                entity.HasOne(f => f.User1).WithMany().HasForeignKey(f => f.UserId1).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(f => f.User2).WithMany().HasForeignKey(f => f.UserId2).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(f => f.Initiator).WithMany().HasForeignKey(f => f.InitiatorId);
            });

            modelBuilder.Entity<FriendshipLabel>(entity =>
            {
                entity.Property(fl => fl.Id).HasColumnType("char(36)").IsRequired();

                entity.HasIndex(fl => fl.FriendshipId);
                entity.HasIndex(fl => fl.CreatorId);

                entity.HasOne(fl => fl.Friendship).WithMany(f => f.Labels).HasForeignKey(fl => fl.FriendshipId);
                entity.HasOne(fl => fl.Creator).WithMany().HasForeignKey(fl => fl.CreatorId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.Property(c => c.Id).HasColumnType("char(36)").IsRequired();
                entity.Property(c => c.Type).HasConversion<string>();
                entity.Property(c => c.CreatedAt).IsRequired();

                entity.HasIndex(c => c.CreatorId);
                entity.HasIndex(c => c.Type);
                entity.HasIndex(c => c.UpdatedAt);

                entity.HasOne(c => c.Creator).WithMany().HasForeignKey(c => c.CreatorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.LastMessage).WithOne().HasForeignKey<Chat>(c => c.LastMessageId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ChatImage>(entity =>
            {
                entity.Property(ci => ci.Id).HasColumnType("char(36)").IsRequired();

                entity.HasIndex(ci => ci.ChatId);

                entity.HasOne(ci => ci.Chat).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatMember>(entity =>
            {
                entity.Property(cm => cm.Id).HasColumnType("char(36)").IsRequired();
                entity.Property(cm => cm.Role).HasConversion<string>();

                entity.HasIndex(cm => cm.ChatId);
                entity.HasIndex(cm => cm.MemberId);
                entity.HasIndex(cm => new { cm.ChatId, cm.MemberId }).IsUnique();

                entity.HasOne(cm => cm.Chat).WithMany(c => c.Members).HasForeignKey(cm => cm.ChatId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(cm => cm.Member).WithMany().HasForeignKey(cm => cm.MemberId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.Property(cm => cm.Id).HasColumnType("char(36)").IsRequired();
                entity.Property(cm => cm.SystemMessageType).HasConversion<string>();
                entity.Property(cm => cm.CreatedAt).IsRequired();

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
