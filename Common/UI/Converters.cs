using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trivial.Maths;
using Trivial.Text;
using Trivial.Web;
using Windows.Devices.Input;
using Windows.UI.Notifications;

namespace Trivial.UI;

/// <summary>
/// The two-way converter of visibility and boolean.
/// </summary>
public sealed class BooleanVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertFromBoolean(VisualUtility.ConvertToBoolean(value), targetType, true);

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => Convert(value, targetType, parameter, language);
}

/// <summary>
/// The visibility converter about the value equaling to the parameter.
/// </summary>
public class ParameterToVisibleConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertFromBoolean(value == parameter, targetType, true);

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertToBoolean(value) == true ? parameter : VisualUtility.ConvertFromBoolean(false, parameter?.GetType(), true);
}

/// <summary>
/// The visibility converter about the value NOT equaling to the parameter.
/// </summary>
public class ParameterToCollapseConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertFromBoolean(value != parameter, targetType, true);

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertToBoolean(value) == false ? parameter : VisualUtility.ConvertFromBoolean(false, parameter?.GetType(), true);
}

/// <summary>
/// The visibility converter about the value is not null.
/// </summary>
public class NullToCollapseConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertFromBoolean(value is not null, targetType, true);

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertToBoolean(value) != true ? null : CreateValue(parameter, language);

    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result created to convert back.</returns>
    protected virtual object CreateValue(object parameter, string language)
        => DependencyProperty.UnsetValue;
}

/// <summary>
/// The visibility converter about the value is null.
/// </summary>
public class NullToVisibleConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertFromBoolean(value is null, targetType, true);

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertToBoolean(value) != false ? null : CreateValue(parameter, language);

    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result created to convert back.</returns>
    protected virtual object CreateValue(object parameter, string language)
        => DependencyProperty.UnsetValue;
}

/// <summary>
/// The visibility converter about the value is not null or empty.
/// </summary>
public class EmptyToCollapseConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertFromBoolean(value is string s ? !string.IsNullOrEmpty(s) : value is not null, targetType, true);

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertToBoolean(value) != true ? null : CreateValue(parameter, language);

    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result created to convert back.</returns>
    protected virtual object CreateValue(object parameter, string language)
        => DependencyProperty.UnsetValue;
}

/// <summary>
/// The visibility converter about the value is null or empty.
/// </summary>
public class EmptyToVisibleConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertFromBoolean(value is string s ? string.IsNullOrEmpty(s) : value is null, targetType, true);

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => VisualUtility.ConvertToBoolean(value) != false ? null : CreateValue(parameter, language);

    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result created to convert back.</returns>
    protected virtual object CreateValue(object parameter, string language)
        => DependencyProperty.UnsetValue;
}

/// <summary>
/// The two-way converter of visibility and boolean.
/// </summary>
public sealed class StrokeSideConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the default length.
    /// </summary>
    public double DefaultLength { get; set; } = 0d;

    /// <summary>
    /// Gets or sets if the left length of the thicknessis using the number from parameter.
    /// </summary>
    public bool Left { get; set; }

    /// <summary>
    /// Gets or sets if the top length of the thickness is using the number from parameter.
    /// </summary>
    public bool Top { get; set; }

    /// <summary>
    /// Gets or sets if the right length of the thickness is using the number from parameter.
    /// </summary>
    public bool Right { get; set; }

    /// <summary>
    /// Gets or sets if the bottom length of the thickness is using the number from parameter.
    /// </summary>
    public bool Bottom { get; set; }

    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not double d)
        {
            if (parameter is null) d = 0d;
            else if (parameter is Thickness thickness)
            {
                if (targetType == typeof(Thickness)) return thickness;
                d = 0d;
                if (Left) d = Math.Max(d, thickness.Left);
                if (Top) d = Math.Max(d, thickness.Top);
                if (Right) d = Math.Max(d, thickness.Right);
                if (Bottom) d = Math.Max(d, thickness.Bottom);
            }
            else if (parameter is int i) d = i;
            else if (parameter is long l) d = l;
            else if (parameter is float f) d = f;
            else if (parameter is JsonDoubleNode j1) d = (double)j1;
            else if (parameter is JsonIntegerNode j2) d = (double)j2;
            else if (parameter is JsonDecimalNode j3) d = (double)j3;
            else return DependencyProperty.UnsetValue;
        }

        if (targetType is null || targetType == typeof(Thickness)) return GetThickness(d);
        if (targetType == typeof(double)) return d;
        try
        {
            if (targetType == typeof(int)) return (int)Math.Round(d);
        }
        catch (InvalidCastException)
        {
            return DependencyProperty.UnsetValue;
        }
        catch (OverflowException)
        {
            return DependencyProperty.UnsetValue;
        }

        try
        {
            if (targetType == typeof(long)) return (long)Math.Round(d);
        }
        catch (InvalidCastException)
        {
            return DependencyProperty.UnsetValue;
        }
        catch (OverflowException)
        {
            return DependencyProperty.UnsetValue;
        }

        if (targetType == typeof(float)) return (float)d;
        return DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => Convert(value, targetType, parameter, language);

    private double GetLength(bool useGiven, double given)
        => useGiven ? given : DefaultLength;

    private Thickness GetThickness(double value)
        => double.IsNaN(value) ? new(DefaultLength) : new(GetLength(Left, value), GetLength(Top, value), GetLength(Right, value), GetLength(Bottom, value));
}

/// <summary>
/// The two-way converter of URI to image source.
/// </summary>
public sealed class UriToBitmapImageSourceConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ImageSource source)
        {
            Uri uri2 = null;
            if (source is BitmapImage bitmap)
                uri2 = bitmap.UriSource;
            else if (source is SvgImageSource svg)
                uri2 = svg.UriSource;
            else
                return DependencyProperty.UnsetValue;

            if (targetType == typeof(Uri)) return uri2;
            else if (targetType == typeof(string)) return uri2?.OriginalString;
            else if (targetType == typeof(ImageSource)) return source;
            return DependencyProperty.UnsetValue;
        }

        if (value is not Uri uri)
        {
            if (value is string s) uri = StringExtensions.TryCreateUri(s);
            else return DependencyProperty.UnsetValue;
        }

        if (string.IsNullOrWhiteSpace(uri?.OriginalString)) return null;
        if (targetType == typeof(ImageSource))
        {
            try
            {
                source = (ImageSource)XamlBindingHelper.ConvertValue(typeof(ImageSource), uri.OriginalString);
                if (source != null) return source;
            }
            catch (Exception)
            {
            }

            return new BitmapImage
            {
                UriSource = uri
            };
        }

        if (targetType == typeof(BitmapImage) || targetType == typeof(BitmapSource)) return new BitmapImage
        {
            UriSource = uri
        };
        if (targetType == typeof(SvgImageSource)) return new SvgImageSource
        {
            UriSource = uri
        };
        if (targetType == typeof(Uri)) return uri;
        if (targetType == typeof(string)) return uri.OriginalString;
        return DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => Convert(value, targetType, parameter, language);
}

/// <summary>
/// The one-way converter of message time string.
/// </summary>
public sealed class MessageTimeOneWayConverter : IValueConverter
{
    /// <summary>
    /// Converts a source to target.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not DateTime time)
        {
            if (value is DateTimeOffset dto)
            {
                time = dto.DateTime;
            }
            else if (value is string s)
            {
                var dt = WebFormat.ParseDate(s);
                if (dt.HasValue) time = dt.Value;
                else return DependencyProperty.UnsetValue;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        var culture = CultureInfo.CurrentUICulture ?? CultureInfo.CurrentCulture;
        if (!string.IsNullOrWhiteSpace(language)) culture = CultureInfo.GetCultureInfo(language);
        var now = DateTime.Now;
        if (now.Year != time.Year)
        {
            var key = GetCultureFamilyName(culture);
            if (key == "zh")
            {
                var year = now.Year - time.Year;
                switch (year)
                {
                    case 1:
                        return time.ToString("'去年'M'月'd'日'HH:mm");
                    case -1:
                        return time.ToString("'明年'M'月'd'日'HH:mm");
                }
            }
            else if (key == "ja")
            {
                var year = now.Year - time.Year;
                switch (year)
                {
                    case 1:
                        return time.ToString("'昨年'M'月'd'日'HH:mm");
                    case -1:
                        return time.ToString("'明年'M'月'd'日'HH:mm");
                }
            }

            return time.ToString("g");
        }

        var days = (int)(now.Date - time.Date).TotalDays;
        switch (days)
        {
            case 0:
                {
                    return time.ToString("T");
                }
            case 1:
                {
                    var abbr = GetYesterdayAbbreviation(culture);
                    if (abbr != null) return string.Concat(abbr, ' ', time.ToString("t"));
                    break;
                }
            case -1:
                {
                    var abbr = GetTomorrowAbbreviation(culture);
                    if (abbr != null) return string.Concat(abbr, ' ', time.ToString("t"));
                    break;
                }
            case 2:
                {
                    var key = GetCultureFamilyName(culture);
                    if (key == "zh") return string.Concat("前天 ", time.ToString("t"));
                    break;
                }
            case -2:
                {
                    var key = GetCultureFamilyName(culture);
                    if (key == "zh") return string.Concat("后天 ", time.ToString("t"));
                    break;
                }
        }

        var pattern = culture?.DateTimeFormat?.MonthDayPattern ?? "MMM dd";
        var cultureKey = GetCultureFamilyName(culture);
        if (cultureKey == "zh") return time.ToString("M'月'd'日'HH:mm");
        pattern = pattern.Replace("MMMM", "MMM");
        return string.Concat(time.ToString(pattern), ' ', time.ToString("t"));
    }

    /// <summary>
    /// Converts the source back.
    /// </summary>
    /// <param name="value">The input value to convert back.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="language">The language code.</param>
    /// <returns>The result converted back.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => DependencyProperty.UnsetValue;

    private static string GetCultureFamilyName(CultureInfo culture)
    {
        var key = culture?.Name;
        if (string.IsNullOrWhiteSpace(key)) return null;
        var i = key.IndexOf('-');
        if (i > 0) key = key.Substring(0, i);
        return key;
    }

    private static string GetYesterdayAbbreviation(CultureInfo culture)
    {
        var key = GetCultureFamilyName(culture);
        return key switch
        {
            "en" => "YTD",
            "fr" => "hier",
            "zh" => "昨天",
            "ja" => "昨日",
            "ko" => "어제",
            //"la" => "hesterno",
            "es" => "ayer",
            "pt" => "ontem",
            "de" => "gestern",
            "el" => "εχθές",
            _ => null
        };
    }

    private static string GetTomorrowAbbreviation(CultureInfo culture)
    {
        var key = GetCultureFamilyName(culture);
        return key switch
        {
            "en" => "TMR",
            "fr" => "demain",
            "zh" => "明天",
            "ja" => "明日",
            "ko" => "내일",
            "la" => "cras",
            "es" => "mañana",
            "pt" => "amanhã",
            "de" => "morgen",
            "el" => "αύριο",
            _ => null
        };
    }
}
