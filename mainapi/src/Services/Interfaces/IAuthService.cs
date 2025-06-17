using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Requests;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> Login(LoginRequest loginRequest);
        //Task<string> Logout();
        Task Register(RegisterRequest registerRequest);
    }
}
