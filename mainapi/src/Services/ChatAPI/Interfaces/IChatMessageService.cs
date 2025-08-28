using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.ChatAPI.Interfaces
{
    public interface IChatMessageService
    {
        Task<ServiceResult<IEnumerable<ChatMessageDTO>>> GetChatMessages(Guid userId, Guid chatId, int page = 1, int pageSize = 100);
        Task<ServiceResult<ChatMessageDTO>> CreateMessage(ChatMessageRequest chatMessageRequest, Guid senderId);
    }
}
