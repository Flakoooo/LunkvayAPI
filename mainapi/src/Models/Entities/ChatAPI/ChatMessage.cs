using LunkvayAPI.src.Models.Enums.ChatEnum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities.ChatAPI
{
    [Table("chat_messages")]
    public class ChatMessage
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("chat_id")]
        [Required]
        public required Guid ChatId { get; set; }

        [Column("sender_id")]
        public Guid SenderId { get; set; }

        [Column("system_message_type")]
        public required SystemMessageType SystemMessageType { get; set; }

        [Column("message")]
        [Required]
        public required string Message { get; set; }

        [Column("is_edited")]
        public bool IsEdited { get; set; }

        [Column("is_pinned")]
        public bool IsPinned { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

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
