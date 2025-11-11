using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("profiles")]
    public class Profile
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public required Guid UserId { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("about")]
        public string? About { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }



        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
