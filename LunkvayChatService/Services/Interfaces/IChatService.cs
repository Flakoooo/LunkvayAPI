using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayChatService.Models.DTO;

namespace LunkvayAPI.src.Services.ChatAPI.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResult<IEnumerable<ChatDTO>>> GetRooms(Guid userId);
        Task<ServiceResult<ChatDTO>> CreateRoom(ChatRequest chatRequest, Guid? creatorId = null);
    }
}
