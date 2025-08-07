using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<UserDTO>> GetUserById(Guid userId);
        Task<ServiceResult<User?>> GetUserByEmail(string email);
        Task<ServiceResult<IEnumerable<UserDTO>>> GetUsers();
        Task<ServiceResult<User>> CreateUser(User user);
    }
}
