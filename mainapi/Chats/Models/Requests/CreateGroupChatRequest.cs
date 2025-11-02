using LunkvayAPI.Common.DTO;
using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class CreateGroupChatRequest
    {
        [Required]
        public required string Name { get; init; }

        [Required]
        public required IList<UserDTO> Members { get; init; }
    }
}
