using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResult<IEnumerable<ChatDTO>>> GetRooms(Guid userId);
        Task<ServiceResult<ChatDTO>> CreateRoom(ChatRequest chatRequest, Guid? creatorId = null);
        Task JoinInRoom(Guid roomId, Guid userId);
        Task InviteInRoom(Guid roomId, Guid senderId, Guid newMemberId);
        Task LeaveFromRoom(Guid roomId, Guid userId);
    }
}
