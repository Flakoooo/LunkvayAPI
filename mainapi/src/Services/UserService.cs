using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Services
{
    public class UserService(LunkvayDBContext lunkvayDBContext) : IUserService
    {
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<User?> Authenticate(string email, string password)
        {
            var user = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User?> Register(RegisterRequest registerRequest)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == registerRequest.Email))
                throw new ArgumentException("Пользователь с данной почтой уже существует");

            var user = new User {  
                Email = registerRequest.Email, 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password), 
                FirstName = registerRequest.FirstName ?? "", 
                LastName = registerRequest.LastName ?? "" 
            };
            var result = await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<User> GetUserById(Guid userId)
        {
            //var user = _users.Find(u => u.Id == userId);
            var user = await _dbContext.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            return user ?? throw new Exception("Пользователь не найден");
        }
    }
}
