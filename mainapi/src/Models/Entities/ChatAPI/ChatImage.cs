using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities.ChatAPI
{
    [Table("chat_images")]
    public class ChatImage
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("chat_id")]
        public required Guid ChatId { get; set; }

        [Column("file_name")]
        public required string FileName { get; set; }

        [Column("updated_at")]
        public DateTime UpdateAt { get; set; }



        [ForeignKey("ChatId")]
        public virtual Chat? Chat { get; set; }
    }
}
