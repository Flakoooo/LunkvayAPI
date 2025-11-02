using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("chats")]
    public class Chat
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("creator_id")]
        public Guid? CreatorId { get; set; }

        [Column("last_message_id")]
        public Guid? LastMessageId { get; set; }

        [Column("name")]
        public string? Name { get; set; } = null;

        [Column("type")]
        public required ChatType Type { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }



        [ForeignKey("CreatorId")]
        public virtual User? Creator { get; set; }

        [ForeignKey("LastMessageId")]
        public virtual ChatMessage? LastMessage { get; set; }


        public virtual ICollection<ChatMember> Members { get; set; } = [];
        public virtual ICollection<ChatMessage> Messages { get; set; } = [];
    }
}
