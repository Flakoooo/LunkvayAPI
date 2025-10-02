using LunkvayIdentityService.Models.Entities;
using LunkvayIdentityService.Models.Requests;
using LunkvayIdentityService.Models.Utils;

namespace LunkvayIdentityService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<string>> Login(LoginRequest loginRequest);
        //Task<string> Logout();
        Task<ServiceResult<User>> Register(RegisterRequest registerRequest);
    }
}
