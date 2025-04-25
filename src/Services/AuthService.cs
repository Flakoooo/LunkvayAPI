using LunkvayAPI.src.Models.Entities;
using LunkvayAPI.src.Models.Requests;
using LunkvayAPI.src.Models.Utils;
using LunkvayAPI.src.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.OAuth;
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
        private readonly IUserService _userService; // Сервис для проверки пользователя

        public AuthService(IOptions<JwtSettings> jwtOptions, IUserService userService)
        {
            _userService = userService;
            _jwtSettings = jwtOptions.Value;

            if (string.IsNullOrEmpty(_jwtSettings.Key)) 
                throw new ArgumentNullException("JWT Key is missing!");
        }

        public async Task<string> Login(LoginRequest loginRequest)
        {
            // проверка существует ли пользователь
            User? user = await _userService.Authenticate(loginRequest.Email, loginRequest.Password) 
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            // создание данных (claims) пользователя
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.GivenName, user.FirstName ?? ""),
                new(ClaimTypes.Surname, user.LastName ?? "")
            };

            // получение ключа шифрования токена (сделать позже в отдельном файле)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _jwtSettings.Key! ?? throw new InvalidOperationException("JWT Key not found."))
            );

            // генерация токена
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // 15 минут
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
