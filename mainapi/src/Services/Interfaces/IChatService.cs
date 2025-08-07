using LunkvayAPI.src.Models.Entities.ChatAPI;
using LunkvayAPI.src.Models.Utils;
using LunkvayApp.src.Models.Enums;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResult<Chat>> CreateRoom(string name, ChatType type);
        Task JoinInRoom(Guid roomId, Guid userId);
        Task InviteInRoom(Guid roomId, Guid senderId, Guid newMemberId);
        Task LeaveFromRoom(Guid roomId, Guid userId);
    }
}
