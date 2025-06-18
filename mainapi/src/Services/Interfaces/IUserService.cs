using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Requests;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IUserService
    {
        public Task<User?> Authenticate(string email, string password);
        public Task<User> Register(RegisterRequest registerRequest);
        public Task<User> GetUserById(Guid userId);
    }
}
