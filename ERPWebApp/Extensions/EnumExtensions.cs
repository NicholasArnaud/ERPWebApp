using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ERPWebApp.Extensions
{
    public static class EnumExtensions
    {
        private static string GetEnumDescription<TEnum>(this TEnum value)
            where TEnum : struct, Enum
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        public static List<SelectListItem> ToList<TEnum>(this TEnum value , string defaultText = "Choose an option")
             where TEnum : struct, Enum
        {
            var list = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new SelectListItem
                {
                    Value = Convert.ToInt32(e).ToString(),
                    Text = e.GetEnumDescription()
                })
                .ToList();

            if (!string.IsNullOrEmpty(defaultText))
            {
                list.Insert(0, new SelectListItem { Value = "-1", Text = defaultText });
            }

            return list;
        }
        
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.GetName() ?? enumValue.ToString();
        }
    }
}
