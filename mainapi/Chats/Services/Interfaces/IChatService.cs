using LunkvayAPI.Chats.Models.DTO;
using LunkvayAPI.Chats.Models.Requests;
using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResult<List<ChatDTO>>> GetRooms(Guid userId);
        Task<ServiceResult<ChatDTO>> CreateRoom(ChatRequest chatRequest, Guid? creatorId = null);
    }
}
