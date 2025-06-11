using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Services.Interfaces;

namespace LunkvayAPI.src.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users =
            [
                new User { Id = "1", Email = "ryan.gosling@gmail.com",      PasswordHash = HashPassword("realhero"),        FirstName = "Райан",    LastName = "Гослинг"    },
                new User { Id = "2", Email = "rinat.goslinov@gmail.com",    PasswordHash = HashPassword("realhero"),        FirstName = "Ринат",    LastName = "Гослинов"   },
                new User { Id = "3", Email = "christian.bale@gmail.com",    PasswordHash = HashPassword("fordvsferrari"),   FirstName = "Кристиан", LastName = "Бейл"       },
                new User { Id = "4", Email = "tom.hardy@gmail.com",         PasswordHash = HashPassword("madmax"),          FirstName = "Том",      LastName = "Харди"      },
                new User { Id = "5", Email = "jake.gyllenhaal@gmail.com",   PasswordHash = HashPassword("realhero"),        FirstName = "Джейк",    LastName = "Джилленхол" }
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
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User> Register(RegisterRequest registerRequest)
        {
            if (_users.Any(u => u.Email == registerRequest.Email))
                throw new ArgumentException("Пользователь с данной почтой уже существует");

            var user = new User { 
                Id = (_users.Count + 1).ToString(), 
                Email = registerRequest.Email, 
                PasswordHash = HashPassword(registerRequest.Password), 
                FirstName = registerRequest.FirstName, 
                LastName = registerRequest.LastName 
            };
            _users.Add(user);
            return user;
        }

        public async Task<User> GetUserById(string userId)
        {
            var user = _users.Find(u => u.Id == userId);
            return user ?? throw new Exception("Пользователь не найден");
        }
    }
}
