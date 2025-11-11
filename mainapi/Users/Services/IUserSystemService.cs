using LunkvayAPI.Data.Entities;

namespace LunkvayAPI.Users.Services
{
    public interface IUserSystemService
    {
        Task<bool> ExistsUserEmail(string email);
        Task<bool> ExistsUserUserName(string userName);

        Task<User?> GetUserById(Guid userId);
        Task<User?> GetUserByEmail(string email);
        Task<User?> CreateUser(
            string userName, string email, string password,
            string firstName, string lastName
        );
    }
}
