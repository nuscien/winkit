using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Media;
using Trivial.Text;
using Windows.UI;

namespace Trivial.UI;

/// <summary>
/// The style for JSON output.
/// </summary>
public class JsonTextStyle : ICloneable
{
    /// <summary>
    /// Initializes a new instance of the JsonTextStyle class.
    /// </summary>
    public JsonTextStyle()
    {
        PropertyForeground = new SolidColorBrush(Color.FromArgb(255, 0xd7, 0xba, 0x7c));
        StringForeground = new SolidColorBrush(Color.FromArgb(255, 0xCE, 0x91, 0x78));
        KeywordForeground = new SolidColorBrush(Color.FromArgb(255, 0x56, 0x9C, 0xD6));
        NumberForeground = new SolidColorBrush(Color.FromArgb(255, 0xB5, 0xCE, 0xA8));
        PunctuationForeground = new SolidColorBrush(Color.FromArgb(255, 0xDC, 0xDC, 0xDC));
    }

    /// <summary>
    /// Initializes a new instance of the JsonTextStyle class.
    /// </summary>
    /// <param name="theme">The application theme.</param>
    /// <param name="isCompact">true if compact the white spaces; otherwise, false.</param>
    public JsonTextStyle(Microsoft.UI.Xaml.ApplicationTheme theme, bool isCompact = false)
    {
        if (theme == Microsoft.UI.Xaml.ApplicationTheme.Dark)
        {
            PropertyForeground = new SolidColorBrush(Color.FromArgb(255, 0xa1, 0xdb, 0xfc));
            StringForeground = new SolidColorBrush(Color.FromArgb(255, 0xcb, 0x92, 0x7b));
            KeywordForeground = new SolidColorBrush(Color.FromArgb(255, 0x52, 0x9b, 0xd3));
            NumberForeground = new SolidColorBrush(Color.FromArgb(255, 0xb7, 0xcd, 0x8e));
            PunctuationForeground = new SolidColorBrush(Color.FromArgb(255, 0xDC, 0xDC, 0xDC));
        }
        else
        {
            PropertyForeground = new SolidColorBrush(Color.FromArgb(255, 0x0f, 0x51, 0xa2));
            StringForeground = new SolidColorBrush(Color.FromArgb(255, 0x9e, 0x20, 0x1c));
            KeywordForeground = new SolidColorBrush(Color.FromArgb(255, 0x00, 0x0e, 0xf9));
            NumberForeground = new SolidColorBrush(Color.FromArgb(255, 0x24, 0x85, 0x9c));
            PunctuationForeground = new SolidColorBrush(Color.FromArgb(255, 0x22, 0x22, 0x22));
        }

        IsCompact = isCompact;
    }

    /// <summary>
    /// Initializes a new instance of the JsonTextStyle class.
    /// </summary>
    /// <param name="propertyForeground">The foreground color of property key.</param>
    /// <param name="stringForeground">The foreground color of string value.</param>
    /// <param name="keywordForeground">The foreground color of language keyword.</param>
    /// <param name="numberForeground">The foreground color of number.</param>
    /// <param name="punctuationForeground">The foreground color of punctuation.</param>
    /// <param name="isCompact">true if compact the white spaces; otherwise, false.</param>
    public JsonTextStyle(Brush propertyForeground, Brush stringForeground, Brush keywordForeground, Brush numberForeground, Brush punctuationForeground, bool isCompact = false)
    {
        PropertyForeground = propertyForeground;
        StringForeground = stringForeground;
        KeywordForeground = keywordForeground;
        NumberForeground = numberForeground;
        PunctuationForeground = punctuationForeground;
        IsCompact = isCompact;
    }


    /// <summary>
    /// Gets or sets the foreground color of property key.
    /// </summary>
    public Brush PropertyForeground { get; set; }

    /// <summary>
    /// Gets or sets the foreground color of string value.
    /// </summary>
    public Brush StringForeground { get; set; }

    /// <summary>
    /// Gets or sets the foreground color of language keyword.
    /// </summary>
    public Brush KeywordForeground { get; set; }

    /// <summary>
    /// Gets or sets the foreground color of number.
    /// </summary>
    public Brush NumberForeground { get; set; }

    /// <summary>
    /// Gets or sets the foreground color of punctuation.
    /// </summary>
    public Brush PunctuationForeground { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether compact the whitespaces.
    /// </summary>
    public bool IsCompact { get; set; }

    /// <summary>
    /// Converts a number to angle model.
    /// </summary>
    /// <param name="value">The raw value.</param>
    public static implicit operator JsonTextStyle(JsonObjectNode value)
    {
        if (value == null) return null;
        var style = new JsonTextStyle();
        var b = TryParse(value, "property") ?? TryParse(value, "prop");
        if (b != null) style.PropertyForeground = b;
        b = TryParse(value, "string") ?? TryParse(value, "str");
        if (b != null) style.StringForeground = b;
        b = TryParse(value, "keyword");
        if (b != null) style.KeywordForeground = b;
        b = TryParse(value, "number") ?? TryParse(value, "num");
        if (b != null) style.NumberForeground = b;
        b = TryParse(value, "punctuation") ?? TryParse(value, "punc");
        if (b != null) style.PunctuationForeground = b;
        if (value.TryGetBooleanValue("compact") == true) style.IsCompact = true;
        return style;
    }

    private static Brush TryParse(JsonObjectNode value, string key)
    {
        var s = value.TryGetStringValue(key)?.Trim();
        if (string.IsNullOrEmpty(s) || !Drawing.ColorCalculator.TryParse(s, out var color)) return null;
        return VisualUtility.ToBrush(color);
    }

    /// <summary>
    /// Clones an object.
    /// </summary>
    /// <returns>The object copied from this instance.</returns>
    public virtual JsonTextStyle Clone()
        => MemberwiseClone() as JsonTextStyle;

    /// <summary>
    /// Clones an object.
    /// </summary>
    /// <returns>The object copied from this instance.</returns>
    object ICloneable.Clone()
        => MemberwiseClone();
}
