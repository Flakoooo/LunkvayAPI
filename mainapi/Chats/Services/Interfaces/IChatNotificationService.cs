using LunkvayAPI.Chats.Models.DTO;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatNotificationService
    {
        Task SendMessage(Guid roomId, ChatMessageDTO message);
    }
}
