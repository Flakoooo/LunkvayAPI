using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities.ChatAPI
{
    [Table("chat_members")]
    public class ChatMember
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("chat_id")]
        public Guid ChatId { get; set; }

        [Column("member_id")]
        public Guid MemberId { get; set; }

        [Column("member_name")]
        public string? MemberName { get; set; }
    }
}
