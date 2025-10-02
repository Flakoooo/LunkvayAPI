using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Enums.ChatEnum;
using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.src.Models.Requests
{
    public class ChatRequest
    {
        public string? Name { get; set; }

        [Required]
        public required ChatType Type { get; set; }

        [Required]
        public required IEnumerable<UserDTO> Members { get; set; }
    }
}
