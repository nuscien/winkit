using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
    public static void WriteOrdered(this StyleConsole? console, int index, string? text = null, string? text2 = null)
    {
        console ??= StyleConsole.Default;
        var indexLen = WriteIndex(console, index).Length;
        var hasText = !string.IsNullOrEmpty(text);
        if (hasText) console.Write(text);
        if (string.IsNullOrEmpty(text2)) return;
        if (hasText) PaddingTwoSpace(console, text!.Length + indexLen);
        console.Write(text2);
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
    public static void WriteOrdered(this StyleConsole? console, int index, ConsoleColor color, string? text, ConsoleColor? color2 = null, string? text2 = null)
    {
        console ??= StyleConsole.Default;
        var indexLen = WriteIndex(console, index).Length;
        var hasText = !string.IsNullOrEmpty(text);
        if (hasText) console.Write(color, text);
        if (string.IsNullOrEmpty(text2)) return;
        if (hasText) PaddingTwoSpace(console, text!.Length + indexLen);
        if (color2.HasValue) console.Write(color2.Value, text2);
        else console.Write(text2);
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
    public static void WriteOrdered(this StyleConsole? console, int index, ConsoleTextStyle? style, string? text, ConsoleTextStyle? style2 = null, string? text2 = null)
    {
        console ??= StyleConsole.Default;
        var indexLen = WriteIndex(console, index).Length;
        var hasText = !string.IsNullOrEmpty(text);
        if (hasText) console.Write(style, text);
        if (string.IsNullOrEmpty(text2)) return;
        if (hasText) PaddingTwoSpace(console, text!.Length + indexLen);
        if (style2 is not null) console.Write(style2, text2);
        else console.Write(text2);
    }

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="text">The text output.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(int index, string? text = null, string? text2 = null)
        => WriteOrdered(StyleConsole.Default, index, text, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="color2">The color of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(int index, ConsoleColor color, string? text, ConsoleColor? color2 = null, string? text2 = null)
        => WriteOrdered(StyleConsole.Default, index, color, text, color2, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="style">The style of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="style2">The style2 of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrdered(int index, ConsoleTextStyle? style, string? text, ConsoleTextStyle? style2 = null, string? text2 = null)
        => WriteOrdered(StyleConsole.Default, index, style, text, style2, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="index">The index of the list item.</param>
    /// <param name="text">The text output.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(this StyleConsole? console, int index, string? text = null, string? text2 = null)
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
    public static void WriteOrderedLine(this StyleConsole? console, int index, ConsoleColor color, string? text, ConsoleColor? color2 = null, string? text2 = null)
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
    public static void WriteOrderedLine(this StyleConsole? console, int index, ConsoleTextStyle? style, string? text, ConsoleTextStyle? style2 = null, string? text2 = null)
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
    public static void WriteOrderedLine(int index, string? text = null, string? text2 = null)
        => WriteOrderedLine(StyleConsole.Default, index, text, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="color2">The color of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(int index, ConsoleColor color, string? text, ConsoleColor? color2 = null, string? text2 = null)
        => WriteOrderedLine(StyleConsole.Default, index, color, text, color2, text2);

    /// <summary>
    /// Writes a number for an ordered list item.
    /// </summary>
    /// <param name="index">The index of the list item.</param>
    /// <param name="style">The style of the text.</param>
    /// <param name="text">The text output.</param>
    /// <param name="style2">The style2 of the second text.</param>
    /// <param name="text2">The second text output after a tab.</param>
    public static void WriteOrderedLine(int index, ConsoleTextStyle? style, string? text, ConsoleTextStyle? style2 = null, string? text2 = null)
        => WriteOrderedLine(StyleConsole.Default, index, style, text, style2, text2);

    /// <summary>
    /// Writes lines in order.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="col">The lines.</param>
    /// <param name="style">The style of line.</param>
    /// <param name="skipEmpty">true if skip empty line; otherwise, false.</param>
    public static void WriteOrderedLine(this StyleConsole? console, IEnumerable<string?>? col, ConsoleTextStyle? style, bool skipEmpty = false)
    {
        if (col is null) return;
        console ??= StyleConsole.Default;
        var i = 1;
        foreach (var line in col)
        {
            if (string.IsNullOrEmpty(line) && skipEmpty) continue;
            WriteOrderedLine(console, i, style, line);
            i++;
        }
    }

    /// <summary>
    /// Writes lines in order.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="col">The lines.</param>
    /// <param name="skipEmpty">true if skip empty line; otherwise, false.</param>
    public static void WriteOrderedLine(this StyleConsole? console, IEnumerable<string?>? col, bool skipEmpty = false)
        => WriteOrderedLine(console, col, null, skipEmpty);

    /// <summary>
    /// Writes lines in order.
    /// </summary>
    /// <param name="console">The console instance.</param>
    /// <param name="col">The lines.</param>
    /// <param name="style1">The style of the first text.</param>
    /// <param name="style2">The style of the second text.</param>
    /// <param name="skipEmpty">true if skip empty line; otherwise, false.</param>
    public static void WriteOrderedLine(this StyleConsole? console, IEnumerable<(string?, string?)>? col, ConsoleTextStyle? style1, ConsoleTextStyle? style2, bool skipEmpty = false)
    {
        if (col is null) return;
        console ??= StyleConsole.Default;
        var i = 1;
        foreach (var line in col)
        {
            if (string.IsNullOrEmpty(line.Item1) && skipEmpty) continue;
            WriteOrderedLine(console, i, style1, line.Item1, style2, line.Item2);
            i++;
        }
    }

    private static string WriteIndex(StyleConsole console, int index)
    {
        var s = index < 10 ? $"{index}.  " : $"{index}. ";
        console.Append(ConsoleColor.Blue, s);
        return s;
    }

    private static void PaddingTwoSpace(StyleConsole console, int textLength)
    {
        var left = console.TryGetCursorLeft();
        if (!left.HasValue) left = textLength;
        if (left.Value < 8) console.Append("\t\t");
        else console.Append(" \t");
    }
}
