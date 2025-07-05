using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LunkvayAPI.src.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUserService _userService;
        private readonly ILogger<AuthService> _logger;


        public AuthService(IOptions<JwtSettings> jwtOptions, IUserService userService, ILogger<AuthService> logger)
        {
            _userService = userService;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;

            string jwtKeyMissing = "JWT ключ не найден!";
            if (string.IsNullOrEmpty(_jwtSettings.Key)) 
                throw new ArgumentNullException(jwtKeyMissing);
        }

        public async Task<string> Login(LoginRequest loginRequest)
        {
            _logger.LogInformation("({Date}) Осуществляется вход для {Email}", DateTime.Now, loginRequest.Email);

            var user = await _userService.GetUserByEmail(loginRequest.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Неверный email или пароль");

            var claims = new List<Claim>
            {
                new("id", user.Id.ToString()),
                new("user_name", user.UserName),
                new("email", user.Email),
                new("first_name", user.FirstName ?? ""),
                new("last_name", user.LastName ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _jwtSettings.Key! ?? throw new InvalidOperationException("JWT ключ не найден"))
            );

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryTime),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> Register(RegisterRequest registerRequest)
        {
            var user = new User
            {
                Email = registerRequest.Email,
                UserName = registerRequest.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                FirstName = registerRequest.FirstName ?? "",
                LastName = registerRequest.LastName ?? ""
            };

            return await _userService.CreateUser(user);
        }
    }
}
