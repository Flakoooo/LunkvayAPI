namespace LunkvayAPI.src.Models.Requests
{
    public class ChatMemberRequest
    {
        public Guid ChatId { get; set; }
        public Guid MemberId { get; set; }
        public Guid InviterId { get; set; }
    }
}
