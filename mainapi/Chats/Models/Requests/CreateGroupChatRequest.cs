using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class CreateGroupChatRequest
    {
        [Required]
        public required string Name { get; init; }

        [Required]
        public required IList<Guid> Members { get; init; }
    }
}
