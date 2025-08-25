using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.ChatAPI.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResult<IEnumerable<ChatDTO>>> GetRooms(Guid userId);
        Task<ServiceResult<IEnumerable<ChatMessageDTO>>> GetChatMessages(Guid userId, Guid chatId, int page = 1, int pageSize = 100);
        Task<ServiceResult<ChatDTO>> CreateRoom(ChatRequest chatRequest, Guid? creatorId = null);
        Task JoinInRoom(Guid roomId, Guid userId);
        Task InviteInRoom(Guid roomId, Guid senderId, Guid newMemberId);
        Task LeaveFromRoom(Guid roomId, Guid userId);
    }
}
