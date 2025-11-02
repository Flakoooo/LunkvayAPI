using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResult<List<ChatDTO>>> GetRooms(Guid userId);
        Task<ServiceResult<ChatDTO>> WriteAndCreatePersonalChat(Guid creatorId, CreatePersonalChatRequest request);
        Task<ServiceResult<ChatDTO>> CreateGroupChat(Guid creatorId, CreateGroupChatRequest chatRequest);
        Task<ServiceResult<ChatDTO>> UpdateChat(Guid userId, Guid chatId, UpdateChatRequest request);
        Task<ServiceResult<bool>> DeleteChat(Guid creatorId, Guid chatId);
    }
}
