using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Friends.Models.DTO
{
    public class FriendDTO
    {
        public Guid? FriendshipId { get; set; }
        public FriendshipStatus? Status { get; set; }
        public List<string?>? Labels { get; set; }
        public Guid? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsOnline { get; set; }
    }
}
            