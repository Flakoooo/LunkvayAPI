using LunkvayAPI.Auth.Models.Enums;
using LunkvayAPI.Auth.Models.Requests;
using LunkvayAPI.Auth.Models.Utils;
using LunkvayAPI.Common.Enums.ErrorCodes;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Common.Utils;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Profiles.Services;
using LunkvayAPI.Users.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace LunkvayAPI.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;
        private readonly IUserSystemService _userService;
        private readonly IProfileSystemService _profileService;


        public AuthService(
            IOptions<JwtSettings> jwtOptions, 
            ILogger<AuthService> logger,
            IUserSystemService userService, 
            IProfileSystemService profileService
        )
        {
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
            _userService = userService;
            _profileService = profileService;

            if (string.IsNullOrEmpty(_jwtSettings.Key))
                throw new ArgumentNullException(AuthErrorCode.JwtKeyMissing.GetDescription());
        }

        public async Task<ServiceResult<string>> Login(LoginRequest loginRequest)
        {
            _logger.LogInformation("({Date}) Осуществляется вход для {Email}", DateTime.Now, loginRequest.Email);

            var user = await _userService.GetUserByEmail(loginRequest.Email);
            if (user is null ||
                !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash)
            ) 
                return ServiceResult<string>.Failure(
                    AuthErrorCode.InvalidCredentials.GetDescription(), 
                    HttpStatusCode.UnprocessableContent
                );

            var claims = new List<Claim>() 
            {
                new("id", user.Id.ToString()),
                new("user_name", user.UserName),
                new("email", user.Email),
                new("first_name", user.FirstName ?? ""),
                new("last_name", user.LastName ?? "")
            };

            if (string.IsNullOrEmpty(_jwtSettings.Key))
                return ServiceResult<string>.Failure(
                    AuthErrorCode.JwtKeyMissing.GetDescription(), 
                    HttpStatusCode.InternalServerError
                );

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var token = new JwtSecurityToken(
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
            if (await _userService.ExistsUserEmail(registerRequest.Email))
                return ServiceResult<User>.Failure(
                    UsersErrorCode.EmailAlreadyExists.GetDescription(),
                    HttpStatusCode.Conflict
                );

            if (await _userService.ExistsUserUserName(registerRequest.UserName))
                return ServiceResult<User>.Failure(
                    UsersErrorCode.UsernameAlreadyExists.GetDescription(),
                    HttpStatusCode.Conflict
                );

            var user = await _userService.CreateUser(
                registerRequest.UserName, registerRequest.Email, registerRequest.Password,
                registerRequest.FirstName ?? "", registerRequest.LastName ?? ""
            );
            if (user is null)
                return ServiceResult<User>.Failure(
                    AuthErrorCode.RegistrationFailed.GetDescription(), 
                    HttpStatusCode.InternalServerError
                );

            if (await _profileService.CreateProfile(user.Id) is null)
                return ServiceResult<User>.Failure(
                    AuthErrorCode.ProfileCreationFailed.GetDescription(),
                    HttpStatusCode.InternalServerError
                );

            return ServiceResult<User>.Success(user);
        }
    }
}
