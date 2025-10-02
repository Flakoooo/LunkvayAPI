namespace LunkvayAPI.Common.DTO
{
    public record class UserListItemDTO
    {
        public required Guid UserId { get; set; }
        public required string FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsOnline { get; set; }
    }
}
