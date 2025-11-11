using LunkvayAPI.Common.Results;
using LunkvayAPI.Data;
using LunkvayAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunkvayAPI.Users.Services
{
    public class UserSystemService(
        LunkvayDBContext lunkvayDBContext
    ) : IUserSystemService
    {
        private readonly LunkvayDBContext _dbContext = lunkvayDBContext;

        public async Task<bool> ExistsUserEmail(string email)
            => await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Email == email);

        public async Task<bool> ExistsUserUserName(string userName)
            => await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.UserName == userName);

        public async Task<User?> GetUserById(Guid userId)
            => await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<User?> GetUserByEmail(string email)
            => await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> CreateUser(
            string userName, string email, string password,
            string firstName, string lastName
        )
        {
            if (string.IsNullOrWhiteSpace(userName) 
                || string.IsNullOrWhiteSpace(email) 
                || string.IsNullOrWhiteSpace(password)
            )
                return null;
            
            var user = User.Create(userName, email, password, firstName, lastName);

            await _dbContext.Users.AddAsync(user);

            await _dbContext.SaveChangesAsync();

            return user;
        }
    }
}
