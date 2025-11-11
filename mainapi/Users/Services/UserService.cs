using LunkvayAPI.Common.DTO;
using LunkvayAPI.Common.Enums.ErrorCodes;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data;
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

        public async Task<ServiceResult<List<UserDTO>>> GetUsers()
        {
            var result = await _dbContext.Users
                .AsNoTracking()
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreatedAt = u.CreatedAt,
                    IsDeleted = u.IsDeleted,
                    LastLogin = u.LastLogin,
                    IsOnline = u.IsOnline
                })
                .ToListAsync();

            return ServiceResult<List<UserDTO>>.Success(result);
        }
    }
}
