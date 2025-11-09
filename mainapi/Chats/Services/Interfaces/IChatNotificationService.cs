using LunkvayAPI.Chats.Models.DTO;

namespace LunkvayAPI.Chats.Services.Interfaces
{
    public interface IChatNotificationService
    {
        Task UpdateChat(Guid roomId, ChatDTO updatedChat);
        Task DeleteChat(Guid roomId, Guid chatId);

        // =======

        Task UpdateMember(Guid roomId, ChatMemberDTO updatedMember);
        Task DeleteMember(Guid roomId, Guid memberId);

        // =======

        Task SendMessage(Guid roomId, ChatMessageDTO message);
        Task UpdateMessage(Guid roomId, ChatMessageDTO updatedMessage);
        Task DeleteMessage(Guid roomId, Guid messageId);
        Task PinMessage(
            Guid roomId, Guid messageId, bool isPinned, DateTime? updatedAt
        );
    }
}
