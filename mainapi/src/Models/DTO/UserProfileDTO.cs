namespace LunkvayAPI.src.Models.DTO
{
    public record class UserProfileDTO
    {
        public required string Id { get; set; }
        public required UserDTO User { get; set; }
        public string? Status { get; set; }
        public string? About { get; set; }
        public int? FriendsCount { get; set; }
        public IEnumerable<UserListItemDTO>? Friends { get; set; }


        public static UserProfileDTO Create(
            string id, UserDTO user, string? status, string? about, int? friendsCount, IEnumerable<UserListItemDTO>? friends
        )
        {
            return new()
            {
                Id = id,
                User = user,
                Status = status,
                About = about,
                FriendsCount = friendsCount,
                Friends = friends
            };
        }
    }
}
