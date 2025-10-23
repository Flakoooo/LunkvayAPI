using System.ComponentModel;

namespace LunkvayAPI.Users.Models.Enums
{
    public enum UsersErrorCode
    {
        [Description("Пользователь не найден")]
        UserNotFound,

        [Description("Пользователь с данной почтой уже существует")]
        EmailAlreadyExists,

        [Description("Пользователь с данным именем пользователя уже существует")]
        UsernameAlreadyExists
    }
}
