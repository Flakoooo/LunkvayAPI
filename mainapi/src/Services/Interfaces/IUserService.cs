using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IUserService
    {
        public Task<UserDTO> GetUserById(Guid userId);
        public Task<User?> GetUserByEmail(string email);
        public Task<IEnumerable<UserDTO>> GetUsers();
        public Task<User> CreateUser(User user);
    }
}
