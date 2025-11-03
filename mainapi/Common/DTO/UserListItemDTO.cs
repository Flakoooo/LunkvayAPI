namespace LunkvayAPI.Common.DTO
{
    public record class UserListItemDTO
    {
        public required Guid UserId { get; set; }
        public required string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsOnline { get; set; } = false;
    }
}
