using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Text;

namespace Trivial.CommandLine;

/// <summary>
/// The rendering and interative options to read line from command line.
/// </summary>
public class ReadLineConsoleOptions
{
    /// <summary>
    /// Gets or sets the hint.
    /// </summary>
    public string Hint { get; set; }

    /// <summary>
    /// Gets or sets the prefix. This will show before input cursor.
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the input cursor is in the same line of hint.
    /// </summary>
    public bool HintInline { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether need trim result.
    /// </summary>
    public bool NeedTrim { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether ask to type again if the input is empty.
    /// The hint will not show again but prefix will output.
    /// </summary>
    public bool AgainIfEmpty { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether skip output a white space between prefix and input cursor.
    /// </summary>
    public bool SkipPrefixPadding { get; set; }

    /// <summary>
    /// Gets or sets the foreground (text) color of hint.
    /// </summary>
    public ConsoleColor? HintForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground (text) color of hint.
    /// </summary>
    public Color? HintForegroundRgbColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of hint.
    /// </summary>
    public ConsoleColor? HintBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of hint.
    /// </summary>
    public Color? HintBackgroundRgbColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground (text) color of prefix.
    /// </summary>
    public ConsoleColor? PrefixForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground (text) color of prefix.
    /// </summary>
    public Color? PrefixForegroundRgbColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of prefix.
    /// </summary>
    public ConsoleColor? PrefixBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of prefix.
    /// </summary>
    public Color? PrefixBackgroundRgbColor { get; set; }
}

/// <summary>
/// The extensions for console renderer.
/// </summary>
public static partial class ConsoleRenderExtensions
{
    /// <summary>
    /// Reads the next line of characters from the standard input stream.
    /// </summary>
    /// <param name="cli">The command line interface proxy.</param>
    /// <param name="options">The rendering options.</param>
    /// <returns>The next line of characters from the input stream, or null if no more lines are available.</returns>
    /// <exception cref="IOException">An I/O error occurred.</exception>
    /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The number of characters in the next line of characters is greater than max value of 32-bit integer.</exception>
    public static string ReadLine(this StyleConsole cli, ReadLineConsoleOptions options)
    {
        if (options is null) return cli.ReadLine();
        if (!string.IsNullOrWhiteSpace(options.Hint))
        {
            cli.Write(new ConsoleTextStyle(
                options.HintForegroundRgbColor,
                options.HintForegroundColor,
                options.HintBackgroundRgbColor,
                options.HintBackgroundColor), options.Hint);
            if (!options.HintInline) cli.WriteLine();
        }

        var style = string.IsNullOrEmpty(options.Prefix) ? null : new ConsoleTextStyle(
                options.PrefixForegroundRgbColor,
                options.PrefixForegroundColor,
                options.PrefixBackgroundRgbColor,
                options.PrefixBackgroundColor);
        if (style != null)
        {
            cli.Write(style, options.Prefix);
            if (!options.SkipPrefixPadding) cli.Write(' ');
        }

        var s = cli.ReadLine();
        if (s != null)
        {
            if (options.NeedTrim) s = s.Trim();
            if (!string.IsNullOrEmpty(s)) return s;
        }

        if (options.AgainIfEmpty) return null;
        if (style != null)
        {
            cli.Write(style, options.Prefix);
            if (!options.SkipPrefixPadding) cli.Write(' ');
        }

        s = cli.ReadLine();
        if (s == null) return null;
        if (options.NeedTrim) s = s.Trim();
        return string.IsNullOrEmpty(s) ? null : s;
    }
}
