using System.ComponentModel;

namespace sfa.sale.generator.core;

public static class EnumExtension
{
    public static string GetEnumDescription<TEnum>(this TEnum item) where TEnum : Enum
    {
        return item.GetType().GetField(item.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false)
            .Cast<DescriptionAttribute>()
            .FirstOrDefault()?.Description ?? string.Empty;
    }
}