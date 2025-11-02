using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatMessageService
    {
        Task<ServiceResult<ChatMessage>> CreateSystemChatMessage(
            Guid chatId, string message, SystemMessageType type
        );


        Task<ServiceResult<List<ChatMessageDTO>>> GetChatMessages(
            Guid userId, Guid chatId, int page = 1, int pageSize = 100
        );
        Task<ServiceResult<List<ChatMessageDTO>>> GetPinnedChatMessages(
            Guid userId, Guid chatId, int page = 1, int pageSize = 100
        );
        Task<ServiceResult<ChatMessageDTO>> CreateMessage(
            Guid senderId, CreateChatMessageRequest request
        );
        Task<ServiceResult<ChatMessageDTO>> EditChatMessage(
            Guid editorId, UpdateEditChatMessageRequest request
        );
        Task<ServiceResult<ChatMessageDTO>> PinChatMessage(
            Guid initiatorId, UpdatePinChatMessageRequest request
        );
        Task<ServiceResult<bool>> DeleteMessage(
            Guid initiatorId, DeleteChatMessageRequest request
        );
    }
}
