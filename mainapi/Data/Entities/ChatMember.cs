using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.Data.Entities
{
    [Table("chat_members")]
    public class ChatMember
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("chat_id")]
        [Required]
        public required Guid ChatId { get; set; }

        [Column("member_id")]
        [Required]
        public required Guid MemberId { get; set; }

        [Column("member_name")]
        public string? MemberName { get; set; } = null;

        [Column("role")]
        [Required]
        public required ChatMemberRole Role { get; set; }



        [ForeignKey("ChatId")]
        public virtual Chat? Chat { get; set; }

        [ForeignKey("MemberId")]
        public virtual User? Member { get; set; }
    }
}
