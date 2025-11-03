using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("chat_images")]
    public class ChatImage
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("chat_id")]
        public required Guid ChatId { get; set; }

        [Column("file_name")]
        public required string FileName { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }



        [ForeignKey("ChatId")]
        public virtual Chat? Chat { get; set; }
    }
}
