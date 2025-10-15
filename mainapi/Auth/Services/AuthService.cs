using LunkvayAPI.Auth.Models.Requests;
using LunkvayAPI.Auth.Models.Utils;
using LunkvayAPI.Common.Results;
using LunkvayAPI.Data.Entities;
using LunkvayAPI.Data.Enums;
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
        private readonly IUserService _userService;
        private readonly IProfileService _profileService;


        public AuthService(
            IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger,
            IUserService userService, IProfileService profileService
        )
        {
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
            _userService = userService;
            _profileService = profileService;

            if (string.IsNullOrEmpty(_jwtSettings.Key))
                throw new ArgumentNullException(ErrorCode.JWTKeyMissing.GetDescription());
        }

        public async Task<ServiceResult<string>> Login(LoginRequest loginRequest)
        {
            _logger.LogInformation("({Date}) Осуществляется вход для {Email}", DateTime.Now, loginRequest.Email);

            ServiceResult<User?> result = await _userService.GetUserByEmail(loginRequest.Email);
            if (!result.IsSuccess || result.Result is null || 
                !BCrypt.Net.BCrypt.Verify(loginRequest.Password, result.Result.PasswordHash)
            ) 
                return ServiceResult<string>.Failure(
                    ErrorCode.IncorrectLoginData.GetDescription(), HttpStatusCode.UnprocessableContent
                );

            User user = result.Result;
            List<Claim> claims =
            [
                new("id", user.Id.ToString()),
                new("user_name", user.UserName),
                new("email", user.Email),
                new("first_name", user.FirstName ?? ""),
                new("last_name", user.LastName ?? "")
            ];

            if (string.IsNullOrEmpty(_jwtSettings.Key))
                return ServiceResult<string>.Failure(
                    ErrorCode.JWTKeyMissing.GetDescription(), 
                    HttpStatusCode.InternalServerError
                );

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
            ServiceResult<User> userResult = await _userService.CreateUser(
                registerRequest.UserName, registerRequest.Email, registerRequest.Password,
                registerRequest.FirstName ?? "", registerRequest.LastName ?? ""
            );
            if (!userResult.IsSuccess || userResult.Result is null)
                return userResult.Error is not null
                    ? ServiceResult<User>.Failure(userResult.Error, userResult.StatusCode)
                    : ServiceResult<User>.Failure(
                        ErrorCode.RegisterFailed.GetDescription(), HttpStatusCode.InternalServerError
                    );

            ServiceResult<Profile> profileResult = await _profileService.CreateProfile(userResult.Result.Id);
            if (!profileResult.IsSuccess || profileResult.Result is null)
                return userResult.Error is not null
                    ? ServiceResult<User>.Failure(userResult.Error, userResult.StatusCode)
                    : ServiceResult<User>.Failure(
                        ErrorCode.ProfileCreateFailed.GetDescription(),
                        HttpStatusCode.InternalServerError
                    );

            return ServiceResult<User>.Success(userResult.Result);
        }
    }
}
