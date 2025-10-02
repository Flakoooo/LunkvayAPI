using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("chats")]
    public class Chat
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("creator_id")]
        public Guid? CreatorId { get; set; }

        [Column("last_message_id")]
        public Guid? LastMessageId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("type")]
        [Required]
        public ChatType Type { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }



        [ForeignKey("CreatorId")]
        public virtual User? Creator { get; set; }

        [ForeignKey("LastMessageId")]
        public virtual ChatMessage? LastMessage { get; set; }


        public virtual ICollection<ChatMember> Members { get; set; } = [];
        public virtual ICollection<ChatMessage> Messages { get; set; } = [];
    }
}
