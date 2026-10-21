using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace SteamWishlistWidget.Converters;

/// <summary>
/// Liga um grupo de RadioButtons a uma propriedade: marcado quando o valor é igual ao ConverterParameter,
/// e ao marcar grava o ConverterParameter (convertido para o tipo da propriedade).
/// </summary>
public sealed class ValueEqualsConverter : IValueConverter
{
    public static readonly ValueEqualsConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Equals(
            System.Convert.ToString(value, CultureInfo.InvariantCulture),
            System.Convert.ToString(parameter, CultureInfo.InvariantCulture),
            StringComparison.Ordinal);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not true || parameter is null)
            return BindingOperations.DoNothing;

        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
        return System.Convert.ChangeType(parameter, type, CultureInfo.InvariantCulture);
    }
}
