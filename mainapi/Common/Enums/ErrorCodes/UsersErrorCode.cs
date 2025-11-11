using System.ComponentModel;

namespace LunkvayAPI.Common.Enums.ErrorCodes
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
