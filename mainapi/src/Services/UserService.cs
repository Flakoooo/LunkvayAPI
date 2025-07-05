using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.src.Services
{
    public class UserService(LunkvayDBContext lunkvayDBContext) : IUserService
    {
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        private static UserDTO BuildUserDTO(User user) => new()
        {
            Id = user.Id.ToString(),
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        public async Task<UserDTO> GetUserById(Guid userId) 
            => await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => BuildUserDTO(u))
            .FirstOrDefaultAsync()
            ?? throw new Exception("Пользователь не найден");

        public async Task<IEnumerable<UserDTO>> GetUsers() 
            => await _dbContext.Users.Select(u => BuildUserDTO(u)).ToListAsync();

        public async Task<User?> GetUserByEmail(string email)
            => await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User> CreateUser(User user)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == user.Email))
                throw new ArgumentException("Пользователь с данной почтой уже существует");

            if (await _dbContext.Users.AnyAsync(u => u.UserName == user.UserName))
                throw new ArgumentException("Пользователь с данным именем пользователя уже существует");

            var result = await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }
    }
}
