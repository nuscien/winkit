using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Maths;
using Trivial.Text;
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
