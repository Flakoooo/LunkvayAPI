namespace LunkvayAPI.Friends.Models.Requests
{
    public class CreateFriendshipLabelRequest
    {
        public required Guid FriendshipId { get; init; }
        public required string Label { get; init; }
    }
}
