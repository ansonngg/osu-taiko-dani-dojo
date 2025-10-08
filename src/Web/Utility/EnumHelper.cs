using System.Text;

namespace OsuTaikoDaniDojo.Web.Utility;

public static class EnumHelper
{
    public static string ToSnakeCase(this Enum value)
    {
        var text = value.ToString();

        if (text.Length < 2)
        {
            return text.ToLowerInvariant();
        }

        var stringBuilder = new StringBuilder(text[..1]);

        for (var i = 1; i < text.Length; ++i)
        {
            if (char.IsUpper(text[i]) && !char.IsUpper(text[i - 1]))
            {
                stringBuilder.Append('_');
            }

            stringBuilder.Append(text[i]);
        }

        return stringBuilder.ToString().ToLowerInvariant();
    }
}
