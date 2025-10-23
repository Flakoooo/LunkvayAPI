using System.ComponentModel;

namespace LunkvayAPI.Common.Utils
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            if (fieldInfo == null) return enumValue.ToString();

            var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0
                ? ((DescriptionAttribute)attributes[0]).Description
                : enumValue.ToString();
        }
    }
}
