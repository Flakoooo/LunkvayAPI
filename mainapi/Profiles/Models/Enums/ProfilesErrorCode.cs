using System.ComponentModel;

namespace LunkvayAPI.Profiles.Models.Enums
{
    public enum ProfilesErrorCode
    {
        [Description("Профиль не найден")]
        ProfileNotFound,

        [Description("Найден профиль без пользователя")]
        ProfileWithoutUser
    }
}
