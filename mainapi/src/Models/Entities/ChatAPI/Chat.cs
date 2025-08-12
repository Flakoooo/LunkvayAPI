using LunkvayAPI.src.Models.Enums.ChatEnum;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities.ChatAPI
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

        [Column("chat_image_name")]
        public string? ChatImageName { get; set; }

        [Column("type")]
        [Required]
        public ChatType Type { get; set; }

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


        public static Chat Create(
            Guid? creatorId, Guid? lastMessageId, string? name, string? chatImageName, ChatType Type
        ) => new()
        {
            CreatorId = creatorId,
            LastMessageId = lastMessageId,
            Name = name,
            ChatImageName = chatImageName,
            Type = Type
            //CreatedAt в базе данных
            //UpdatedAt в базе данных
        };
    }
}
