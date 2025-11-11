using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Users.Services
{
    public interface IUserService
    {
        Task<ServiceResult<UserDTO>> GetUserById(Guid userId);
        Task<ServiceResult<List<UserDTO>>> GetUsers();
    }
}
