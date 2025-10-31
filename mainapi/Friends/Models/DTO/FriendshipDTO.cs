using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Friends.Models.DTO
{
    public class FriendshipDTO
    {
        public required Guid FriendshipId { get; set; }
        public FriendshipStatus? Status { get; set; }
        public List<FriendshipLabelDTO>? Labels { get; set; }
        public required Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsOnline { get; set; }
    }
}
            