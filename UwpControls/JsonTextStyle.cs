using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Trivial.Text;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Trivial.UI;

/// <summary>
/// The style for JSON output.
/// </summary>
public class JsonTextStyle : ICloneable
{
    /// <summary>
    /// The foreground color of property key.
    /// </summary>
    private static readonly Brush propertyForeground = new SolidColorBrush(Color.FromArgb(255, 0xCE, 0x91, 0x78));

    /// <summary>
    /// The foreground color of string value.
    /// </summary>
    private static readonly Brush stringForeground = new SolidColorBrush(Color.FromArgb(255, 0xCE, 0x91, 0x78));

    /// <summary>
    /// The foreground color of language keyword.
    /// </summary>
    private static readonly Brush keywordForeground = new SolidColorBrush(Color.FromArgb(255, 0x56, 0x9C, 0xD6));

    /// <summary>
    /// The foreground color of number.
    /// </summary>
    private static readonly Brush numberForeground = new SolidColorBrush(Color.FromArgb(255, 0xB5, 0xCE, 0xA8));

    /// <summary>
    /// The foreground color of punctuation.
    /// </summary>
    private static readonly Brush punctuationForeground = new SolidColorBrush(Color.FromArgb(255, 0xDC, 0xDC, 0xDC));

    /// <summary>
    /// Gets or sets the foreground color of property key.
    /// </summary>
    public Brush PropertyForeground { get; set; } = propertyForeground;

    /// <summary>
    /// Gets or sets the foreground color of string value.
    /// </summary>
    public Brush StringForeground { get; set; } = stringForeground;

    /// <summary>
    /// Gets or sets the foreground color of language keyword.
    /// </summary>
    public Brush KeywordForeground { get; set; } = keywordForeground;

    /// <summary>
    /// Gets or sets the foreground color of number.
    /// </summary>
    public Brush NumberForeground { get; set; } = numberForeground;

    /// <summary>
    /// Gets or sets the foreground color of punctuation.
    /// </summary>
    public Brush PunctuationForeground { get; set; } = punctuationForeground;

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
        return VisualUtilities.ToBrush(color);
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
