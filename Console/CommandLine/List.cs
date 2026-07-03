using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Trivial.CommandLine;

/// <summary>
/// The command line interface.
/// </summary>
public static partial class DefaultConsole
{
    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="index">The index of the list item.</param>
    /// <param name="text">The text output.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(this StyleConsole console, int index, string text = null, string text2 = null)
    {
        console ??= StyleConsole.Default;
        console.Write(ConsoleColor.Blue, index);
        console.Write(ConsoleColor.Blue, index < 10 ? ".  " : ". ");
        if (!string.IsNullOrEmpty(text)) console.Write(text);
        if (!string.IsNullOrEmpty(text2)) console.Write($" \t{text2}");
    }

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="index">The index of the list item.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="color2">The color of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(this StyleConsole console, int index, ConsoleColor color, string text, ConsoleColor? color2 = null, string text2 = null)
    {
        console ??= StyleConsole.Default;
        console.Write(ConsoleColor.Blue, index);
        console.Write(ConsoleColor.Blue, index < 10 ? ".  " : ". ");
        if (!string.IsNullOrEmpty(text)) console.Write(color, text);
        if (!string.IsNullOrEmpty(text2))
        {
            console.Write(" \t");
            if (color2.HasValue) console.Write(color2.Value, text2);
            else console.Write(text2);
        }
    }

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="index">The index of the list item.</param>
    /// <param name="style">The style of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="style2">The style2 of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(this StyleConsole console, int index, ConsoleTextStyle style, string text, ConsoleTextStyle style2 = null, string text2 = null)
    {
        console ??= StyleConsole.Default;
        console.Write(ConsoleColor.Blue, index);
        console.Write(ConsoleColor.Blue, index < 10 ? ".  " : ". ");
        if (!string.IsNullOrEmpty(text)) console.Write(style, text);
        if (!string.IsNullOrEmpty(text2))
        {
            console.Write(" \t");
            if (style2 is not null) console.Write(style2, text2);
            else console.Write(text2);
        }
    }

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="text">The text output.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(int index, string text = null, string text2 = null)
        => WriteOrdered(StyleConsole.Default, index, text, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="color2">The color of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(int index, ConsoleColor color, string text, ConsoleColor? color2 = null, string text2 = null)
        => WriteOrdered(StyleConsole.Default, index, color, text, color2, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="style">The style of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="style2">The style2 of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(int index, ConsoleTextStyle style, string text, ConsoleTextStyle style2 = null, string text2 = null)
        => WriteOrdered(StyleConsole.Default, index, style, text, style2, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="index">The index of the list item.</param>
    /// <param name="text">The text output.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(this StyleConsole console, int index, string text = null, string text2 = null)
    {
        console ??= StyleConsole.Default;
        WriteOrdered(console, index, text, text2);
        console.WriteLine();
    }

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="index">The index of the list item.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="color2">The color of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(this StyleConsole console, int index, ConsoleColor color, string text, ConsoleColor? color2 = null, string text2 = null)
    {
        console ??= StyleConsole.Default;
        WriteOrdered(console, index, color, text, color2, text2);
        console.WriteLine();
    }

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="index">The index of the list item.</param>
    /// <param name="style">The style of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="style2">The style2 of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(this StyleConsole console, int index, ConsoleTextStyle style, string text, ConsoleTextStyle style2 = null, string text2 = null)
    {
        console ??= StyleConsole.Default;
        WriteOrdered(console, index, style, text, style2, text2);
        console.WriteLine();
    }

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="text">The text output.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(int index, string text = null, string text2 = null)
        => WriteOrderedLine(StyleConsole.Default, index, text, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="color2">The color of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(int index, ConsoleColor color, string text, ConsoleColor? color2 = null, string text2 = null)
        => WriteOrderedLine(StyleConsole.Default, index, color, text, color2, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="style">The style of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="style2">The style2 of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(int index, ConsoleTextStyle style, string text, ConsoleTextStyle style2 = null, string text2 = null)
        => WriteOrderedLine(StyleConsole.Default, index, style, text, style2, text2);
}
