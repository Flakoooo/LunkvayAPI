using LunkvayAPI.src.Models.Entities;

namespace LunkvayAPI.src.Services.Interfaces
{
    public interface IUserService
    {
        public Task<User?> Authenticate(string email, string password);
    }
}
