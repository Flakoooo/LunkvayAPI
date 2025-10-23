using LunkvayAPI.Data.Enums;

namespace LunkvayAPI.Friends.Models.Requests
{
    public class UpdateFriendshipStatusRequest
    {
        public required FriendshipStatus Status { get; init; }
    }
}
