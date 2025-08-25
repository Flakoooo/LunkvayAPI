namespace LunkvayAPI.src.Models.DTO
{
    public record class UserProfileDTO
    {
        public required Guid Id { get; set; }
        public required UserDTO User { get; set; }
        public string? Status { get; set; }
        public string? About { get; set; }
        public int? FriendsCount { get; set; }
        public IEnumerable<UserListItemDTO>? Friends { get; set; }
    }
}
