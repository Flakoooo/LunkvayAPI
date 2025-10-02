using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatMessageService
    {
        Task<ServiceResult<IEnumerable<ChatMessageDTO>>> GetChatMessages(Guid userId, Guid chatId, int page = 1, int pageSize = 100);
        Task<ServiceResult<ChatMessageDTO>> CreateMessage(ChatMessageRequest chatMessageRequest, Guid senderId);
    }
}
