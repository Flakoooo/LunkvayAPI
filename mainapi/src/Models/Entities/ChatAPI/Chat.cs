using LunkvayApp.src.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities.ChatAPI
{
    [Table("chats")]
    public class Chat
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("type")]
        [Required]
        public ChatType Type { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("chat_image_name")]
        public string? ChatImageName { get; set; }
    }
}
