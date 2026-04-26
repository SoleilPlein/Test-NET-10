using System.Globalization;
using System.Numerics;

namespace Test_NET_10;

public interface INumberParser
{
    bool TryParseFlexible<T>(string? input, IFormatProvider formatProvider, out T value)
    where T : struct, INumber<T>;
}

public sealed class FlexibleNumberParser : INumberParser
{
    public bool TryParseFlexible<T>(string? input, IFormatProvider formatProvider, out T value)
        where T : struct, INumber<T>
    {
        if (T.TryParse(input, formatProvider, out value))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(input) && input.Contains('.') &&
            T.TryParse(input, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = default;
        return false;
    }
}

public static class NumberParser
{
    private static readonly FlexibleNumberParser parser = new();

    public static bool TryParseFlexible<T>(string? input, IFormatProvider formatProvider, out T value)
        where T : struct, INumber<T>
    {
        return parser.TryParseFlexible(input, formatProvider, out value);
    }
}
