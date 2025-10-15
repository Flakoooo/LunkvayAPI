using System.ComponentModel;
using System.Reflection;

namespace LunkvayAPI.Data.Enums
{
    public enum ErrorCode
    {
        [Description("JWT ключ не найден!")]
        JWTKeyMissing,

        [Description("Неверный email или пароль")]
        IncorrectLoginData,

        [Description("Ошибка при регистрации пользователя")]
        RegisterFailed,

        [Description("Пользователь зарегистрирован, но произошла непредвиденная ошибка при создании профиля пользователя")]
        ProfileCreateFailed,

        [Description("Id пользователя не может быть пустым")]
        UserIdIsNull,

        [Description("Пользователь не найден")]
        UserNotFound,

        [Description("Пользователь с данной почтой уже существует")]
        EmailAlreadyTaken,

        [Description("Пользователь с данным именем пользователя уже существует")]
        UserNameAlreadyTaken,

        [Description("Профиль не найден")]
        ProfileNotFound,

        [Description("Найден профиль без пользователя")]
        ProfileWithoutUser,

        [Description("Путь к изображениям пользователя не указан или отсуствует файл конфигурации")]
        UsersAvatarsNotFound,

        [Description("Ошибка сервера")]
        InternalServerError
    }

    public static class ErrorCodeExtensions
    {
        public static string GetDescription(this ErrorCode errorCode)
            => errorCode.GetType().GetField(errorCode.ToString())?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description 
            ?? errorCode.ToString();
    }
}
