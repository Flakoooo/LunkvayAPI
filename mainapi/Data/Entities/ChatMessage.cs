using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("chat_messages")]
    public class ChatMessage
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("chat_id")]
        public required Guid ChatId { get; set; }

        [Column("sender_id")]
        public Guid SenderId { get; set; }

        [Column("system_message_type")]
        public required SystemMessageType SystemMessageType { get; set; }

        [Column("message")]
        public required string Message { get; set; }

        [Column("is_edited")]
        public bool IsEdited { get; set; } = false;

        [Column("is_pinned")]
        public bool IsPinned { get; set; } = false;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("pinned_at")]
        public DateTime? PinnedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }


        [ForeignKey("ChatId")]
        public virtual Chat? Chat { get; set; }

        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }
    }
}
