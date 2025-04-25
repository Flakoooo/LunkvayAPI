using LunkvayAPI.src.Models.Requests;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> Login(LoginRequest loginRequest);
    }
}
