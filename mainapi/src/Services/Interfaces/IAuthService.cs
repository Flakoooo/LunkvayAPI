using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<string>> Login(LoginRequest loginRequest);
        //Task<string> Logout();
        Task<ServiceResult<User>> Register(RegisterRequest registerRequest);
    }
}
