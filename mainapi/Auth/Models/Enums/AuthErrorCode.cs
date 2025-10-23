using System.ComponentModel;

namespace LunkvayAPI.Auth.Models.Enums
{
    public enum AuthErrorCode
    {
        [Description("JWT ключ не найден!")]
        JwtKeyMissing,

        [Description("Неверный email или пароль")]
        InvalidCredentials,

        [Description("Ошибка при регистрации пользователя")]
        RegistrationFailed,

        [Description("Пользователь зарегистрирован, но произошла непредвиденная ошибка при создании профиля пользователя")]
        ProfileCreationFailed
    }
}
