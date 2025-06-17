using LunkvayAPI.src.Models.Entities;

namespace LunkvayAPI.src.Models.DTO
{
    public record class UserDTO
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public static UserDTO ConvertUserToDTO(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}
