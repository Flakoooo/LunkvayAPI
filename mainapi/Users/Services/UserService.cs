using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
using LunkvayAPI.Users.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LunkvayAPI.Users.Services
{
    public class UserService(LunkvayDBContext lunkvayDBContext) : IUserService
    {
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<ServiceResult<UserDTO>> GetUserById(Guid userId)
        {
            if (userId == Guid.Empty)
                return ServiceResult<UserDTO>.Failure(ErrorCode.UserIdRequired.GetDescription());

            var result = await _dbContext.Users
                    .AsNoTracking()
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
                return ServiceResult<UserDTO>.Failure(UsersErrorCode.UserNotFound.GetDescription(), HttpStatusCode.NotFound);

            return ServiceResult<UserDTO>.Success(result);
        }

        public async Task<ServiceResult<User?>> GetUserByEmail(string email)
        {
            var result = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (result is null)
                return ServiceResult<User?>.Failure(UsersErrorCode.UserNotFound.GetDescription(), HttpStatusCode.NotFound);

            return ServiceResult<User?>.Success(result);
        }

        public async Task<ServiceResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var result = await _dbContext.Users
                .AsNoTracking()
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
            if (await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email))
                return ServiceResult<User>.Failure(UsersErrorCode.EmailAlreadyExists.GetDescription(), HttpStatusCode.Conflict);

            if (await _dbContext.Users.AsNoTracking().AnyAsync(u => u.UserName == userName))
                return ServiceResult<User>.Failure(UsersErrorCode.UsernameAlreadyExists.GetDescription(), HttpStatusCode.Conflict);

            var user = User.Create(userName, email, password, firstName, lastName);

            await _dbContext.Users.AddAsync(user);

            await _dbContext.SaveChangesAsync();

            return ServiceResult<User>.Success(user);
        }
    }
}
