using LunkvayAPI.Chats.Models.DTO;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatNotificationService
    {
        Task UserInvited(Guid roomId, string inviterName, string newMemberName);

        Task UserJoined(Guid roomId, string userName);

        Task UserLeft(Guid roomId, string userName);

        Task SendMessage(Guid roomId, ChatMessageDTO message);
    }
}
