using LunkvayApp.src.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.src.Models.Requests
{
    public class ChatRequest
    {
        public string? Name { get; set; }

        [Required]
        public ChatType Type { get; set; }
    }
}
