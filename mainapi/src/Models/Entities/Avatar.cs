using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities
{
    [Table("avatars")]
    public class Avatar
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public required Guid UserId { get; set; }

        [Column("file_name")]
        public required string FileName { get; set; } //user123.png

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }



        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
