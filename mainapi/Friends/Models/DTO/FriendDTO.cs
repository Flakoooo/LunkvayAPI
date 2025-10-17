namespace LunkvayAPI.Friends.Models.DTO
{
    public class FriendDTO
    {
        public Guid FriendshipId { get; set; }
        public required Guid UserId { get; set; }
        public required string FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsOnline { get; set; }
    }
}
