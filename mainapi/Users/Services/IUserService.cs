using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;

namespace LunkvayAPI.Users.Services
{
    public interface IUserService
    {
        Task<ServiceResult<UserDTO>> GetUserById(Guid userId);
        Task<ServiceResult<User?>> GetUserByEmail(string email);
        Task<ServiceResult<IEnumerable<UserDTO>>> GetUsers();
        Task<ServiceResult<User>> CreateUser(User user);
    }
}
