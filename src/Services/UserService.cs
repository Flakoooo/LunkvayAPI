using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Services.Interfaces;

namespace LunkvayAPI.src.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users =
            [
                new User { Id = "1", Email = "ryan.gosling@gmail.com", PasswordHash = HashPassword("realhero"), FirstName = "Райан", LastName = "Гослинг" }
            ];

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<User?> Authenticate(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return null;

            // Проверяем пароль (в реальном проекте используйте BCrypt)
            if (user.PasswordHash != HashPassword(password))
                return null;

            return user;
        }
    }
}
