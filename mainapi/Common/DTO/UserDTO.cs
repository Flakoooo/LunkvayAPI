namespace LunkvayAPI.Common.DTO
{
    public record class UserDTO
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsOnline { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
