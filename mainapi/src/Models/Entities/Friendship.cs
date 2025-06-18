using LunkvayAPI.src.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities
{
    [Table("friendships")]
    public class Friendship
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id_1")]
        [Required]
        public required Guid UserId1 { get; set; }

        [Column("user_id_2")]
        [Required]
        public required Guid UserId2 { get; set; }

        [Column("status")]
        [Required]
        public required FriendshipStatus Status { get; set; }

        [Column("initiator_id")]
        [Required]
        public required Guid InitiatorId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }



        [ForeignKey("UserId1")]
        public virtual User? User1 { get; set; }

        [ForeignKey("UserId2")]
        public virtual User? User2 { get; set; }

        [ForeignKey("InitiatorId")]
        public virtual User? Initiator { get; set; }

        public virtual ICollection<FriendshipLabel> Labels { get; set; } = [];
    }
}
