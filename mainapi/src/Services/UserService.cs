using LunkvayAPI.src.Models.DTO;
using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using LunkvayAPI.src.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net;

namespace LunkvayAPI.src.Services
{
    public class UserService(LunkvayDBContext lunkvayDBContext) : IUserService
    {
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<ServiceResult<UserDTO>> GetUserById(Guid userId)
        {
            UserDTO? result = await _dbContext.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new UserDTO
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        CreatedAt = u.CreatedAt,
                        IsDeleted = u.IsDeleted,
                        LastLogin = u.LastLogin,
                        IsOnline = u.IsOnline
                    })
                    .FirstOrDefaultAsync();

            if (result is null)
                return ServiceResult<UserDTO>.Failure("Пользователь не найден", HttpStatusCode.NotFound);

            return ServiceResult<UserDTO>.Success(result);
        }

        public async Task<ServiceResult<User?>> GetUserByEmail(string email)
        {
            User? result = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (result is null)
                return ServiceResult<User?>.Failure("Пользователь не найден", HttpStatusCode.NotFound);

            return ServiceResult<User?>.Success(result);
        }

        public async Task<ServiceResult<IEnumerable<UserDTO>>> GetUsers()
        {
            List<UserDTO> result = await _dbContext.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreatedAt = u.CreatedAt,
                    IsDeleted = u.IsDeleted,
                    LastLogin = u.LastLogin,
                    IsOnline = u.IsOnline
                })
                .ToListAsync();

            return ServiceResult<IEnumerable<UserDTO>>.Success(result);
        }

        public async Task<ServiceResult<User>> CreateUser(User user)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == user.Email))
                return ServiceResult<User>.Failure("Пользователь с данной почтой уже существует", HttpStatusCode.Conflict);

            if (await _dbContext.Users.AnyAsync(u => u.UserName == user.UserName))
                return ServiceResult<User>.Failure("Пользователь с данным именем пользователя уже существует", HttpStatusCode.Conflict);

            EntityEntry<User> result = await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<User>.Success(result.Entity);
        }
    }
}
