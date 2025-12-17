using System;
using System.ComponentModel;
using System.Globalization;

namespace UnityModStudio.Options;

public class EnumDescriptionConverter(Type enumType) : EnumConverter(enumType)
{
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(string);

    public override object? ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object? value, Type destType)
    {
        if (value is not Enum)
            return null;

        var field = EnumType.GetField(Enum.GetName(EnumType, value));
        var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return descriptionAttribute != null ? descriptionAttribute.Description : value.ToString();
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);

    public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object? value)
    {
        if (value is not string str)
            return null;

        foreach (var field in EnumType.GetFields())
        {
            var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            if (descriptionAttribute != null && str == descriptionAttribute.Description)
                return Enum.Parse(EnumType, field.Name);
        }
        return Enum.Parse(EnumType, str);
    }
}