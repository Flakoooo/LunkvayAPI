using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Friends.Models.DTO
{
    public class FriendshipDTO
    {
        public Guid? FriendshipId { get; set; }
        public FriendshipStatus? Status { get; set; }
        public List<FriendshipLabelDTO>? Labels { get; set; }
        public Guid? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsOnline { get; set; }
    }
}
            