using LunkvayAPI.Common.DTO;
using LunkvayAPI.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class ChatRequest
    {
        public string? Name { get; init; }

        [Required]
        public required ChatType Type { get; init; }

        [Required]
        public required IEnumerable<UserDTO> Members { get; init; }
    }
}
