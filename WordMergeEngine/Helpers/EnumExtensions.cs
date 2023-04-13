using System.ComponentModel.DataAnnotations;

namespace WordMergeEngine.Helpers
{
    public static class EnumExtensions
    {
        private static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo.FirstOrDefault()?.GetCustomAttributes(typeof(T), false);
            return (T)attributes?.FirstOrDefault();
        }

        public static string GetDisplayName(this Enum value)
        {
            var attribute = value.GetAttribute<DisplayAttribute>();
            return attribute?.Name;
        }
    }
}
