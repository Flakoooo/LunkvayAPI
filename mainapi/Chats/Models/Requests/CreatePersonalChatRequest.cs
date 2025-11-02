using LunkvayAPI.Common.DTO;
using System.ComponentModel.DataAnnotations;

namespace LunkvayAPI.Chats.Models.Requests
{
    public class CreatePersonalChatRequest
    {
        [Required]
        public required UserDTO Interlocutor { get; set; }

        [Required]
        public required string Message { get; set; }
    }
}
