using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace LunkvayAPI.src.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUserService _userService;
        private readonly ILogger<AuthService> _logger;

        private readonly string jwtKeyMissing = "JWT ключ не найден!";


        public AuthService(IOptions<JwtSettings> jwtOptions, IUserService userService, ILogger<AuthService> logger)
        {
            _userService = userService;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;

            if (_jwtSettings.Key is null or "")
                throw new ArgumentNullException(jwtKeyMissing);
        }

        public async Task<ServiceResult<string>> Login(LoginRequest loginRequest)
        {
            _logger.LogInformation("({Date}) Осуществляется вход для {Email}", DateTime.Now, loginRequest.Email);

            ServiceResult<User?> result = await _userService.GetUserByEmail(loginRequest.Email);
            if (result is null || !result.IsSuccess || result.Result is null || 
                !BCrypt.Net.BCrypt.Verify(loginRequest.Password, result.Result.PasswordHash)
            ) return ServiceResult<string>.Failure("Неверный email или пароль", HttpStatusCode.UnprocessableContent);

            User user = result.Result;
            List<Claim> claims =
            [
                new("id", user.Id.ToString()),
                new("user_name", user.UserName),
                new("email", user.Email),
                new("first_name", user.FirstName ?? ""),
                new("last_name", user.LastName ?? "")
            ];

            if (_jwtSettings.Key is null or "")
                return ServiceResult<string>.Failure(jwtKeyMissing, HttpStatusCode.InternalServerError);

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            JwtSecurityToken token = new(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryTime),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return ServiceResult<string>.Success(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<ServiceResult<User>> Register(RegisterRequest registerRequest)
        {
            User user = User.Create(
                registerRequest.UserName, registerRequest.Email, registerRequest.Password,
                registerRequest.FirstName ?? "", registerRequest.LastName ?? ""
            );

            ServiceResult<User> result = await _userService.CreateUser(user);
            if (result is null)
                return ServiceResult<User>.Failure("Ошибка при регистрации пользователя", HttpStatusCode.InternalServerError);

            if (!result.IsSuccess || result.Result is null)
                return result.Error is not null
                    ? ServiceResult<User>.Failure(result.Error, result.StatusCode)
                    : ServiceResult<User>.Failure("Ошибка при регистрации пользователя", HttpStatusCode.InternalServerError);
            else return ServiceResult<User>.Success(result.Result);
        }
    }
}
