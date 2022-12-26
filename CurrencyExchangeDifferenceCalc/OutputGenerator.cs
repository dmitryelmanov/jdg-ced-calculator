using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CurrencyExchangeDifferenceCalc;

internal static class OutputGenerator
{
    private static CultureInfo Culture = new CultureInfo("pl-PL");

    public static string Generate(Output output, string template)
    {
        var props = typeof(Output).GetTypeInfo().GetProperties()
            .SelectMany(x =>
            {
                if (x.PropertyType == typeof(Output.Row))
                {
                    return x.PropertyType.GetTypeInfo().GetProperties()
                        .Select(p => new Property(p, output, x));
                }

                return new[] { new Property(x, output) };
            });

        var result = template;
        foreach (var prop in props)
        {
            result = Regex.Replace(result!, $"{{{prop.Name}}}", $"{prop.Value}");
        }

        return result!;
    }


    internal sealed class Property
    {
        public Property(PropertyInfo pi, object? obj, PropertyInfo? parent = null)
        {
            Name = parent is null ? pi.Name : $"{parent.Name}.{pi.Name}";
            obj = parent is null ? obj : parent.GetValue(obj);
            if (pi.PropertyType == typeof(decimal))
            {
                Value = Convert.ToDecimal(pi.GetValue(obj)).ToString("F4", Culture);
            }
            else
            {
                Value = Convert.ToString(pi.GetValue(obj), Culture) ?? string.Empty;
            }
        }

        public string Name { get; }
        public string Value { get; }
    }
}
