using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
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
                return ServiceResult<UserDTO>.Failure(ErrorCode.UserIdIsNull.GetDescription());

            UserDTO? result = await _dbContext.Users
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
                return ServiceResult<UserDTO>.Failure(ErrorCode.UserNotFound.GetDescription(), HttpStatusCode.NotFound);

            return ServiceResult<UserDTO>.Success(result);
        }

        public async Task<ServiceResult<User?>> GetUserByEmail(string email)
        {
            User? result = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (result is null)
                return ServiceResult<User?>.Failure(ErrorCode.UserNotFound.GetDescription(), HttpStatusCode.NotFound);

            return ServiceResult<User?>.Success(result);
        }

        public async Task<ServiceResult<IEnumerable<UserDTO>>> GetUsers()
        {
            List<UserDTO> result = await _dbContext.Users
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
                return ServiceResult<User>.Failure(ErrorCode.EmailAlreadyTaken.GetDescription(), HttpStatusCode.Conflict);

            if (await _dbContext.Users.AsNoTracking().AnyAsync(u => u.UserName == userName))
                return ServiceResult<User>.Failure(ErrorCode.UserNameAlreadyTaken.GetDescription(), HttpStatusCode.Conflict);

            User user = User.Create(userName, email, password, firstName, lastName);

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<User>.Success(user);
        }
    }
}
