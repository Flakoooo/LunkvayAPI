using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities.ChatAPI
{
    [Table("chat_messages")]
    public class ChatMessage
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("sender_id")]
        public Guid SenderId { get; set; }

        [Column("message")]
        public required string Message { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
