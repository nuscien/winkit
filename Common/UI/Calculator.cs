using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Trivial.Drawing;
using Windows.UI;

namespace Trivial.UI;

/// <summary>
/// The utilities of visual element.
/// </summary>
public static partial class VisualUtility
{
    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 1.</param>
    /// <returns>A color with new alpha channel value.</returns>
    public static Color Opacity(Color value, double alpha)
        => Color.FromArgb(ToChannel(value.A * alpha), value.R, value.G, value.B);

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 1.</param>
    /// <returns>A color with new alpha channel value.</returns>
    public static Color Opacity(Color value, float alpha)
        => Color.FromArgb(ToChannel(value.A * alpha), value.R, value.G, value.B);

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
    /// <returns>A color with new alpha channel value.</returns>
    public static Color Opacity(Color value, byte alpha)
        => Color.FromArgb(ToChannel(alpha / 255f * value.A), value.R, value.G, value.B);

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 1.</param>
    /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
    /// <returns>A color with new alpha channel value.</returns>
    public static Color Opacity(Color value, double alpha, bool resetOriginalAlpha)
        => Color.FromArgb(resetOriginalAlpha ? ToChannel(alpha * 255) : ToChannel(value.A * alpha), value.R, value.G, value.B);

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 1.</param>
    /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
    /// <returns>A color with new alpha channel value.</returns>
    public static Color Opacity(Color value, float alpha, bool resetOriginalAlpha)
        => Color.FromArgb(resetOriginalAlpha ? ToChannel(alpha * 255) : ToChannel(value.A * alpha), value.R, value.G, value.B);

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
    /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
    /// <returns>A color with new alpha channel value.</returns>
    public static Color Opacity(Color value, byte alpha, bool resetOriginalAlpha)
        => Color.FromArgb(resetOriginalAlpha ? ToChannel(alpha) : ToChannel(alpha / 255f * value.A), value.R, value.G, value.B);

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color collection.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
    /// <returns>A color collection with new alpha channel value.</returns>
    public static IEnumerable<Color> Opacity(IEnumerable<Color> value, double alpha)
        => value?.Select(ele => Opacity(ele, alpha));

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color collection.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
    /// <returns>A color collection with new alpha channel value.</returns>
    public static IEnumerable<Color> Opacity(IEnumerable<Color> value, float alpha)
        => value?.Select(ele => Opacity(ele, alpha));

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color collection.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
    /// <returns>A color collection with new alpha channel value.</returns>
    public static IEnumerable<Color> Opacity(IEnumerable<Color> value, byte alpha)
        => value?.Select(ele => Opacity(ele, alpha));

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color collection.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
    /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
    /// <returns>A color collection with new alpha channel value.</returns>
    public static IEnumerable<Color> Opacity(IEnumerable<Color> value, double alpha, bool resetOriginalAlpha)
        => value?.Select(ele => Opacity(ele, alpha, resetOriginalAlpha));

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color collection.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
    /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
    /// <returns>A color collection with new alpha channel value.</returns>
    public static IEnumerable<Color> Opacity(IEnumerable<Color> value, float alpha, bool resetOriginalAlpha)
        => value?.Select(ele => Opacity(ele, alpha, resetOriginalAlpha));

    /// <summary>
    /// Calculates to get the color with opacity and a given color.
    /// </summary>
    /// <param name="value">The source color collection.</param>
    /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
    /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
    /// <returns>A color collection with new alpha channel value.</returns>
    public static IEnumerable<Color> Opacity(IEnumerable<Color> value, byte alpha, bool resetOriginalAlpha)
        => value?.Select(ele => Opacity(ele, alpha, resetOriginalAlpha));

    /// <summary>
    /// Creates a new color by set a channel to a given color.
    /// </summary>
    /// <param name="value">The base color.</param>
    /// <param name="channel">The channel to set.</param>
    /// <param name="newValue">The new value of channel.</param>
    /// <returns>A color with new channel value.</returns>
    public static Color WithChannel(Color value, ColorChannels channel, byte newValue)
        => Color.FromArgb(
            channel.HasFlag((ColorChannels)8) ? newValue : value.A,
            channel.HasFlag(ColorChannels.Red) ? newValue : value.R,
            channel.HasFlag(ColorChannels.Green) ? newValue : value.G,
            channel.HasFlag(ColorChannels.Blue) ? newValue : value.B);

    /// <summary>
    /// Gets the channel value of a specific color.
    /// </summary>
    /// <param name="value">The color to get the channel.</param>
    /// <param name="channel">The channel to get.</param>
    /// <returns>The channel value.</returns>
    public static byte GetChannelValue(Color value, ColorChannels channel)
    {
        var arr = new List<byte>();
        if (channel.HasFlag((ColorChannels)8)) arr.Add(value.A);
        if (channel.HasFlag(ColorChannels.Red)) arr.Add(value.R);
        if (channel.HasFlag(ColorChannels.Green)) arr.Add(value.G);
        if (channel.HasFlag(ColorChannels.Blue)) arr.Add(value.B);
        if (arr.Count == 0) return 127;
        float count = 0;
        foreach (var n in arr)
        {
            count += n;
        }

        return (byte)Math.Round(count / arr.Count);
    }

    /// <summary>
    /// Inverts RGB.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <returns>The color to reverse.</returns>
    public static Color Invert(Color value)
        => Color.FromArgb(value.A, InvertChannel(value.R), InvertChannel(value.G), InvertChannel(value.B));

    /// <summary>
    /// Inverts RGB.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="ratio">The ratio to change. Value is from 0 to 1.</param>
    /// <returns>The color to reverse.</returns>
    public static Color Invert(Color value, double ratio)
    {
        if (ratio >= 1) return Color.FromArgb(value.A, InvertChannel(value.R), InvertChannel(value.G), InvertChannel(value.B));
        if (ratio <= 0) return value;
        var a = 255 * ratio;
        var b = 1 - 2 * ratio;
        return Color.FromArgb(value.A,
            ToChannel(a + b * value.R),
            ToChannel(a + b * value.G),
            ToChannel(a + b * value.B));
    }

    /// <summary>
    /// Inverts RGB.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="ratio">The ratio to change. Value is from 0 to 1.</param>
    /// <returns>The color to reverse.</returns>
    public static Color Invert(Color value, float ratio)
    {
        if (ratio >= 1) return Color.FromArgb(value.A, InvertChannel(value.R), InvertChannel(value.G), InvertChannel(value.B));
        if (ratio <= 0) return value;
        var a = 255 * ratio;
        var b = 1 - 2 * ratio;
        return Color.FromArgb(value.A,
            ToChannel(a + b * value.R),
            ToChannel(a + b * value.G),
            ToChannel(a + b * value.B));
    }

    /// <summary>
    /// Inverts RGB.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <returns>The color to reverse.</returns>
    public static IEnumerable<Color> Invert(IEnumerable<Color> value)
        => value?.Select(ele => Invert(ele));

    /// <summary>
    /// Inverts RGB.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <param name="ratio">The ratio to change. Value is from 0 to 1.</param>
    /// <returns>The color to reverse.</returns>
    public static IEnumerable<Color> Invert(IEnumerable<Color> value, double ratio)
        => value?.Select(ele => Invert(ele, ratio));

    /// <summary>
    /// Inverts RGB.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <param name="ratio">The ratio to change. Value is from 0 to 1.</param>
    /// <returns>The color to reverse.</returns>
    public static IEnumerable<Color> Invert(IEnumerable<Color> value, float ratio)
        => value?.Select(ele => Invert(ele, ratio));

    /// <summary>
    /// Adjusts color balance.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="channel">The channel to set.</param>
    /// <param name="ratio">The ratio to change. Value is from -1 to 1.</param>
    /// <returns>The color after color balance.</returns>
    public static Color ColorBalance(Color value, ColorChannels channel, double ratio)
    {
        if (ratio == 0) return value;
        var white = ratio > 0 ? 255 : 0;
        var black = ratio < 0 ? 255 : 0;
        ratio = Math.Abs(ratio);
        return Color.FromArgb(
            value.A,
            ToChannel(((channel.HasFlag(ColorChannels.Red) ? white : black) - value.R) * ratio + value.R),
            ToChannel(((channel.HasFlag(ColorChannels.Green) ? white : black) - value.G) * ratio + value.G),
            ToChannel(((channel.HasFlag(ColorChannels.Blue) ? white : black) - value.B) * ratio + value.B));
    }

    /// <summary>
    /// Adjusts color balance.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="channel">The channel to set.</param>
    /// <param name="ratio">The ratio to change. Value is from -1 to 1.</param>
    /// <returns>The color after color balance.</returns>
    public static Color ColorBalance(Color value, ColorChannels channel, float ratio)
    {
        if (ratio == 0) return value;
        var white = ratio > 0 ? 255 : 0;
        var black = ratio < 0 ? 255 : 0;
        ratio = Math.Abs(ratio);
        return Color.FromArgb(
            value.A,
            ToChannel(((channel.HasFlag(ColorChannels.Red) ? white : black) - value.R) * ratio + value.R),
            ToChannel(((channel.HasFlag(ColorChannels.Green) ? white : black) - value.G) * ratio + value.G),
            ToChannel(((channel.HasFlag(ColorChannels.Blue) ? white : black) - value.B) * ratio + value.B));
    }

    /// <summary>
    /// Adjusts color balance.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="redRatio">The ratio to change for red channel. Value is from -1 to 1.</param>
    /// <param name="greenRatio">The ratio to change for green channel. Value is from -1 to 1.</param>
    /// <param name="blueRatio">The ratio to change for blue channel. Value is from -1 to 1.</param>
    /// <returns>The color after color balance.</returns>
    public static Color ColorBalance(Color value, double redRatio, double greenRatio, double blueRatio)
    {
        var red = value.R;
        if (redRatio != 0) red = ToChannel(((redRatio > 0 ? 255 : 0) - red) * Math.Abs(redRatio) + red);
        var green = value.G;
        if (greenRatio != 0) green = ToChannel(((greenRatio > 0 ? 255 : 0) - green) * Math.Abs(greenRatio) + green);
        var blue = value.B;
        if (blueRatio != 0) blue = ToChannel(((blueRatio > 0 ? 255 : 0) - blue) * Math.Abs(blueRatio) + blue);
        return Color.FromArgb(value.A, red, green, blue);
    }

    /// <summary>
    /// Adjusts color balance.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="redRatio">The ratio to change for red channel. Value is from -1 to 1.</param>
    /// <param name="greenRatio">The ratio to change for green channel. Value is from -1 to 1.</param>
    /// <param name="blueRatio">The ratio to change for blue channel. Value is from -1 to 1.</param>
    /// <returns>The color after color balance.</returns>
    public static Color ColorBalance(Color value, float redRatio, float greenRatio, float blueRatio)
    {
        var red = value.R;
        if (redRatio != 0) red = ToChannel(((redRatio > 0 ? 255 : 0) - red) * Math.Abs(redRatio) + red);
        var green = value.G;
        if (greenRatio != 0) green = ToChannel(((greenRatio > 0 ? 255 : 0) - green) * Math.Abs(greenRatio) + green);
        var blue = value.B;
        if (blueRatio != 0) blue = ToChannel(((blueRatio > 0 ? 255 : 0) - blue) * Math.Abs(blueRatio) + blue);
        return Color.FromArgb(value.A, red, green, blue);
    }

    /// <summary>
    /// Adjusts color balance.
    /// </summary>
    /// <param name="value">The source color collection.</param>
    /// <param name="channel">The channel to set.</param>
    /// <param name="ratio">The ratio to change. Value is from -1 to 1.</param>
    /// <returns>The color collection after color balance.</returns>
    public static IEnumerable<Color> ColorBalance(IEnumerable<Color> value, ColorChannels channel, double ratio)
        => value?.Select(ele => ColorBalance(ele, channel, ratio));

    /// <summary>
    /// Adjusts color balance.
    /// </summary>
    /// <param name="value">The source color collection.</param>
    /// <param name="channel">The channel to set.</param>
    /// <param name="ratio">The ratio to change. Value is from -1 to 1.</param>
    /// <returns>The color collection after color balance.</returns>
    public static IEnumerable<Color> ColorBalance(IEnumerable<Color> value, ColorChannels channel, float ratio)
        => value?.Select(ele => ColorBalance(ele, channel, ratio));
    /// <summary>
    /// Increases brighness.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="ratio">The brightness ratio to increase. Value is from -1 to 1.</param>
    /// <returns>The color after lighten.</returns>
    public static Color Lighten(Color value, double ratio)
    {
        if (ratio == 0) return value;
        if (ratio > 1) return Color.FromArgb(value.A, 255, 255, 255);
        if (ratio < -1) return Color.FromArgb(value.A, 0, 0, 0);
        var bg = ratio > 0 ? 255 : 0;
        ratio = Math.Abs(ratio);
        return Color.FromArgb(
            value.A,
            ToChannel((bg - value.R) * ratio + value.R),
            ToChannel((bg - value.G) * ratio + value.G),
            ToChannel((bg - value.B) * ratio + value.B));
    }

    /// <summary>
    /// Increases brighness.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="ratio">The brightness ratio to increase. Value is from -1 to 1.</param>
    /// <returns>The color after lighten.</returns>
    public static Color Lighten(Color value, float ratio)
    {
        if (ratio == 0) return value;
        if (ratio > 1) return Color.FromArgb(value.A, 255, 255, 255);
        if (ratio < -1) return Color.FromArgb(value.A, 0, 0, 0);
        var bg = ratio > 0 ? 255 : 0;
        ratio = Math.Abs(ratio);
        return Color.FromArgb(
            value.A,
            ToChannel((bg - value.R) * ratio + value.R),
            ToChannel((bg - value.G) * ratio + value.G),
            ToChannel((bg - value.B) * ratio + value.B));
    }

    /// <summary>
    /// Increases brighness.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <param name="ratio">The brightness ratio to increase. Value is from -1 to 1.</param>
    /// <returns>The color after lighten.</returns>
    public static IEnumerable<Color> Lighten(IEnumerable<Color> value, double ratio)
        => value?.Select(ele => Lighten(ele, ratio));

    /// <summary>
    /// Increases brighness.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <param name="ratio">The brightness ratio to increase. Value is from -1 to 1.</param>
    /// <returns>The color after lighten.</returns>
    public static IEnumerable<Color> Lighten(IEnumerable<Color> value, float ratio)
        => value?.Select(ele => Lighten(ele, ratio));

    /// <summary>
    /// Decreases brighness.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="ratio">The brightness ratio to decrease. Value is from -1 to 1.</param>
    /// <returns>The color after darken.</returns>
    public static Color Darken(Color value, double ratio)
        => Lighten(value, -ratio);

    /// <summary>
    /// Decreases brighness.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="ratio">The brightness ratio to decrease. Value is from -1 to 1.</param>
    /// <returns>The color after darken.</returns>
    public static Color Darken(Color value, float ratio)
        => Lighten(value, -ratio);

    /// <summary>
    /// Decreases brighness.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <param name="ratio">The brightness ratio to decrease. Value is from -1 to 1.</param>
    /// <returns>The color after darken.</returns>
    public static IEnumerable<Color> Darken(IEnumerable<Color> value, double ratio)
        => value?.Select(ele => Darken(ele, ratio));

    /// <summary>
    /// Decreases brighness.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <param name="ratio">The brightness ratio to decrease. Value is from -1 to 1.</param>
    /// <returns>The color after darken.</returns>
    public static IEnumerable<Color> Darken(IEnumerable<Color> value, float ratio)
        => value?.Select(ele => Darken(ele, ratio));

    /// <summary>
    /// Toggles brightness between light mode and dark mode.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <returns>The color toggled.</returns>
    public static Color ToggleBrightness(Color value)
    {
        var delta = byte.MaxValue
            - Maths.Arithmetic.Max(value.R, value.G, value.B)
            - Maths.Arithmetic.Min(value.R, value.G, value.B);
        return Color.FromArgb(value.A, PlusChannel(value.R, delta), PlusChannel(value.G, delta), PlusChannel(value.B, delta));
    }

    /// <summary>
    /// Toggles brightness between light mode and dark mode.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <param name="level">The relative saturation level.</param>
    /// <returns>The color toggled.</returns>
    public static Color ToggleBrightness(Color value, RelativeBrightnessLevels level)
    {
        var max = Maths.Arithmetic.Max(value.R, value.G, value.B);
        var min = Maths.Arithmetic.Min(value.R, value.G, value.B);
        var high = byte.MaxValue - max;
        var delta = high - min;
        switch (level)
        {
            case RelativeBrightnessLevels.Switch:
                break;
            case RelativeBrightnessLevels.High:
                if (high <= min) return value;
                break;
            case RelativeBrightnessLevels.Low:
                if (high >= min) return value;
                break;
            case RelativeBrightnessLevels.Middle:
                {
                    if (high == min) return value;
                    return Color.FromArgb(
                        value.A,
                        ToChannel(value.R + delta / 2f),
                        ToChannel(value.G + delta / 2f),
                        ToChannel(value.B + delta / 2f));
                }
            case RelativeBrightnessLevels.Exposure:
                if (high == 0) return value;
                delta = high;
                break;
            case RelativeBrightnessLevels.Shadow:
                if (min == 0) return value;
                delta = -min;
                break;
            default:
                return value;
        }

        return Color.FromArgb(value.A, PlusChannel(value.R, delta), PlusChannel(value.G, delta), PlusChannel(value.B, delta));
    }

    /// <summary>
    /// Toggles brightness between light mode and dark mode.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <returns>The color toggled.</returns>
    public static IEnumerable<Color> ToggleBrightness(IEnumerable<Color> value)
        => value?.Select(ele => ToggleBrightness(ele));

    /// <summary>
    /// Toggles brightness between light mode and dark mode.
    /// </summary>
    /// <param name="value">The source color value collection.</param>
    /// <param name="level">The relative saturation level.</param>
    /// <returns>The color toggled.</returns>
    public static IEnumerable<Color> ToggleBrightness(IEnumerable<Color> value, RelativeBrightnessLevels level)
        => value?.Select(ele => ToggleBrightness(ele));

    private static byte InvertChannel(byte value)
        => (byte)(byte.MaxValue - value);

    private static byte PlusChannel(byte left, int right)
        => (byte)(left + right);

    private static byte PlusChannel(ref float c, float delta)
    {
#if NETFRAMEWORK
        var r = (int)Math.Round(c + delta);
#else
        var r = (int)MathF.Round(c + delta);
#endif
        if (r < 0) return byte.MinValue;
        else if (r > 255) return byte.MaxValue;
        return (byte)r;
    }

    private static byte ToChannel(float c)
    {
#if NETFRAMEWORK
        var r = (int)Math.Round(c);
#else
        var r = (int)MathF.Round(c);
#endif
        if (r < 0) return byte.MinValue;
        else if (r > 255) return byte.MaxValue;
        return (byte)r;
    }

    private static byte ToChannel(double c)
    {
        var r = (int)Math.Round(c);
        if (r < 0) return byte.MinValue;
        else if (r > 255) return byte.MaxValue;
        return (byte)r;
    }
}
