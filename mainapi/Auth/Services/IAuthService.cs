using LunkvayAPI.Auth.Models.Requests;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;

namespace LunkvayAPI.Auth.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<string>> Login(LoginRequest loginRequest);
        //Task<string> Logout();
        Task<ServiceResult<User>> Register(RegisterRequest registerRequest);
    }
}
