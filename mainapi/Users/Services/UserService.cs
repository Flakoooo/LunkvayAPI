using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net;

namespace LunkvayAPI.Users.Services
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

        public async Task<ServiceResult<User>> CreateUser(
            string userName, string email, string password,
            string firstName, string lastName
        )
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == email))
                return ServiceResult<User>.Failure("Пользователь с данной почтой уже существует", HttpStatusCode.Conflict);

            if (await _dbContext.Users.AnyAsync(u => u.UserName == userName))
                return ServiceResult<User>.Failure("Пользователь с данным именем пользователя уже существует", HttpStatusCode.Conflict);

            User user = User.Create(userName, email, password, firstName, lastName);

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<User>.Success(user);
        }
    }
}
