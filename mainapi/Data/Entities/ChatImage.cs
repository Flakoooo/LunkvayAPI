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

        [Column("img_db_id")]
        public required string ImgDBId { get; set; }

        [Column("img_db_url")]
        public required string ImgDBUrl { get; set; }

        [Column("img_db_delete_url")]
        public required string ImgDBDeleteUrl { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }



        [ForeignKey("ChatId")]
        public virtual Chat? Chat { get; set; }
    }
}
