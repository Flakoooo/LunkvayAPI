using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("chat_members")]
    public class ChatMember
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("chat_id")]
        public required Guid ChatId { get; set; }

        [Column("member_id")]
        public required Guid MemberId { get; set; }

        [Column("member_name")]
        public string? MemberName { get; set; } = null;

        [Column("role")]
        public required ChatMemberRole Role { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }



        [ForeignKey("ChatId")]
        public virtual Chat? Chat { get; set; }

        [ForeignKey("MemberId")]
        public virtual User? Member { get; set; }
    }
}
