using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("avatars")]
    public class Avatar
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public required Guid UserId { get; set; }

        [Column("img_db_id")]
        public required string ImgDBId { get; set; }

        [Column("img_db_url")]
        public required string ImgDBUrl { get; set; }

        [Column("img_db_delete_url")]
        public required string ImgDBDeleteUrl { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }



        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
