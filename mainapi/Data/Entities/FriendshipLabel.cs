using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("friendship_labels")]
    public class FriendshipLabel
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();  

        [Column("friendship_id")]
        [Required]
        public required Guid FriendshipId { get; set; }

        [Column("creator_id")]
        [Required]
        public required Guid CreatorId { get; set; }

        [Column("label")]
        [Required]
        public required string Label { get; set; }



        [ForeignKey("FriendshipId")]
        public virtual Friendship Friendship { get; set; } = null!;

        [ForeignKey("CreatorId")]
        public virtual User? Creator { get; set; }
    }
}
