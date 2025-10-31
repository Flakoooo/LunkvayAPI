using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("friendships")]
    public class Friendship
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id_1")]
        public required Guid UserId1 { get; set; }

        [Column("user_id_2")]
        public required Guid UserId2 { get; set; }

        [Column("status")]
        public required FriendshipStatus Status { get; set; }

        [Column("initiator_id")]
        public required Guid InitiatorId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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
