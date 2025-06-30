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

            User? user = await _userService.Authenticate(loginRequest.Email, loginRequest.Password) 
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            var claims = new List<Claim>
            {
                new("id", user.Id.ToString()),
                new("email", user.Email),
                new("first_name", user.FirstName ?? ""),
                new("last_name", user.LastName ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _jwtSettings.Key! ?? throw new InvalidOperationException("JWT Key not found."))
            );

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryTime), // время жизни
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User?> Register(RegisterRequest registerRequest)
        {
            var register = _userService.Register(registerRequest);
            return await register;
        }
    }
}
