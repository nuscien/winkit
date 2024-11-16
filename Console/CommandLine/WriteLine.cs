using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using Trivial.Collection;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.CommandLine;

/// <summary>
/// The command line interface.
/// </summary>
public static partial class DefaultConsole
{
    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="content">The text content.</param>
    public static void WriteLine(ConsoleText content = null)
        => StyleConsole.Default.WriteLine(content);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="content1">The text content 1.</param>
    /// <param name="content2">The text content 2.</param>
    /// <param name="additionalContext">The additional text content collection.</param>
    public static void WriteLine(ConsoleText content1, ConsoleText content2, params ConsoleText[] additionalContext)
        => StyleConsole.Default.WriteLine(content1, content2, additionalContext);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="content">The text content collection.</param>
    public static void WriteLine(IEnumerable<ConsoleText> content)
        => StyleConsole.Default.WriteLine(content);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void WriteLine(string s, params object[] args)
        => StyleConsole.Default.WriteLine(s, args);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void WriteLine(ConsoleTextStyle style, string s, params object[] args)
        => StyleConsole.Default.WriteLine(style, s, args);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void WriteLine(ConsoleColor foreground, string s, params object[] args)
        => StyleConsole.Default.WriteLine(foreground, s, args);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, string s, params object[] args)
        => StyleConsole.Default.WriteLine(foreground, background, s, args);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void WriteLine(Color foreground, string s, params object[] args)
        => StyleConsole.Default.WriteLine(foreground, s, args);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void WriteLine(Color foreground, Color background, string s, params object[] args)
        => StyleConsole.Default.WriteLine(foreground, background, s, args);

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void WriteLine(IConsoleTextPrettier style, string s, params object[] args)
        => StyleConsole.Default.WriteLine(style, s, args);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(StringBuilder s)
        => StyleConsole.Default.WriteLine(s);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(ConsoleTextStyle style, StringBuilder s)
        => StyleConsole.Default.WriteLine(style, s);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(ConsoleColor foreground, StringBuilder s)
        => StyleConsole.Default.WriteLine(foreground, s);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, StringBuilder s)
        => StyleConsole.Default.WriteLine(foreground, background, s);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(Color foreground, StringBuilder s)
        => StyleConsole.Default.WriteLine(foreground, s);

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(Color foreground, Color background, StringBuilder s)
        => StyleConsole.Default.WriteLine(foreground, background, s);

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(IConsoleTextPrettier style, StringBuilder s)
        => StyleConsole.Default.WriteLine(style, s?.ToString());

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(SecureString s)
        => StyleConsole.Default.WriteLine(s?.ToUnsecureString());

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(ConsoleTextStyle style, SecureString s)
        => StyleConsole.Default.WriteLine(style, s?.ToUnsecureString());

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(ConsoleColor foreground, SecureString s)
        => StyleConsole.Default.WriteLine(foreground, s?.ToUnsecureString());

    /// <summary>
    /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, SecureString s)
        => StyleConsole.Default.WriteLine(foreground, background, s.ToUnsecureString());

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(int number)
        => StyleConsole.Default.WriteLine(number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(int number, string format)
        => StyleConsole.Default.WriteLine(number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleTextStyle style, int number)
        => StyleConsole.Default.WriteLine(style, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleTextStyle style, int number, string format)
        => StyleConsole.Default.WriteLine(style, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor foreground, int number)
        => StyleConsole.Default.WriteLine(foreground, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleColor foreground, int number, string format)
        => StyleConsole.Default.WriteLine(foreground, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, int number)
        => StyleConsole.Default.WriteLine(foreground, background, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, int number, string format)
        => StyleConsole.Default.WriteLine(foreground, background, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(long number)
        => StyleConsole.Default.WriteLine(number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(long number, string format)
        => StyleConsole.Default.WriteLine(number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleTextStyle style, long number)
        => StyleConsole.Default.WriteLine(style, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleTextStyle style, long number, string format)
        => StyleConsole.Default.WriteLine(style, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor foreground, long number)
        => StyleConsole.Default.WriteLine(foreground, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleColor foreground, long number, string format)
        => StyleConsole.Default.WriteLine(foreground, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, long number)
        => StyleConsole.Default.WriteLine(foreground, background, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, long number, string format)
        => StyleConsole.Default.WriteLine(foreground, background, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ulong number)
        => StyleConsole.Default.WriteLine(number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleTextStyle style, ulong number)
        => StyleConsole.Default.WriteLine(style, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor foreground, ulong number)
        => StyleConsole.Default.WriteLine(foreground, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, ulong number)
        => StyleConsole.Default.WriteLine(foreground, background, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(float number)
        => StyleConsole.Default.WriteLine(number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(float number, string format)
        => StyleConsole.Default.WriteLine(number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleTextStyle style, float number)
        => StyleConsole.Default.WriteLine(style, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleTextStyle style, float number, string format)
        => StyleConsole.Default.WriteLine(style, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor foreground, float number)
        => StyleConsole.Default.WriteLine(foreground, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleColor foreground, float number, string format)
        => StyleConsole.Default.WriteLine(foreground, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, float number)
        => StyleConsole.Default.WriteLine(foreground, background, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, float number, string format)
        => StyleConsole.Default.WriteLine(foreground, background, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(decimal number)
        => StyleConsole.Default.WriteLine(number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleTextStyle style, decimal number)
        => StyleConsole.Default.WriteLine(style, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor foreground, decimal number)
        => StyleConsole.Default.WriteLine(foreground, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, decimal number)
        => StyleConsole.Default.WriteLine(foreground, background, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(double number)
        => StyleConsole.Default.WriteLine(number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(double number, string format)
        => StyleConsole.Default.WriteLine(number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleTextStyle style, double number)
        => StyleConsole.Default.WriteLine(style, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleTextStyle style, double number, string format)
        => StyleConsole.Default.WriteLine(style, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor foreground, double number)
        => StyleConsole.Default.WriteLine(foreground, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleColor foreground, double number, string format)
        => StyleConsole.Default.WriteLine(foreground, number, format);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, double number)
        => StyleConsole.Default.WriteLine(foreground, background, number);

    /// <summary>
    /// Writes the specified number, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, double number, string format)
        => StyleConsole.Default.WriteLine(foreground, background, number, format);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void WriteLine(char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.WriteLine(value, start, count);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void WriteLine(ConsoleTextStyle style, char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.WriteLine(style, value, start, count);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void WriteLine(ConsoleColor foreground, char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.WriteLine(foreground, value, start, count);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.WriteLine(foreground, background, value, start, count);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void WriteLine(IConsoleTextPrettier style, char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.WriteLine(style, value, start, count);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void WriteLine(char value, int repeatCount = 1)
        => StyleConsole.Default.WriteLine(value, repeatCount);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void WriteLine(ConsoleTextStyle style, char value, int repeatCount = 1)
        => StyleConsole.Default.WriteLine(style, value, repeatCount);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void WriteLine(ConsoleColor foreground, char value, int repeatCount = 1)
        => StyleConsole.Default.WriteLine(foreground, value, repeatCount);

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void WriteLine(IConsoleTextPrettier style, char value, int repeatCount = 1)
        => StyleConsole.Default.WriteLine(style, value, repeatCount);

    /// <summary>
    /// Writes the specified characters, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void WriteLine(ConsoleColor? foreground, ConsoleColor? background, char value, int repeatCount = 1)
        => StyleConsole.Default.WriteLine(foreground, background, value, repeatCount);

    /// <summary>
    /// Writes an exception, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="stackTrace">true if output stack trace; otherwise, false.</param>
    public static void WriteLine(Exception ex, bool stackTrace = false)
        => StyleConsole.Default.WriteLine(new ConsoleTextStyle(ConsoleColor.Red), null as ConsoleTextStyle, ex, stackTrace);

    /// <summary>
    /// Writes an exception, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="captionStyle">The style of header.</param>
    /// <param name="messageStyle">The style of details.</param>
    /// <param name="stackTrace">true if output stack trace; otherwise, false.</param>
    public static void WriteLine(ConsoleTextStyle captionStyle, ConsoleTextStyle messageStyle, Exception ex, bool stackTrace = false)
        => StyleConsole.Default.WriteLine(captionStyle, messageStyle, ex, stackTrace);

    /// <summary>
    /// Writes an exception, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="ex">The error information.</param>
    public static void WriteLine(Data.ErrorMessageResult ex)
        => StyleConsole.Default.WriteLine(new ConsoleTextStyle(ConsoleColor.Red), null as ConsoleTextStyle, ex);

    /// <summary>
    /// Writes an exception, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="ex">The error information.</param>
    /// <param name="captionStyle">The style of header.</param>
    /// <param name="messageStyle">The style of details.</param>
    public static void WriteLine(ConsoleTextStyle captionStyle, ConsoleTextStyle messageStyle, Data.ErrorMessageResult ex)
        => StyleConsole.Default.WriteLine(captionStyle, messageStyle, ex);

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="json">The JSON instance.</param>
    public static void WriteLine(IJsonValueNode json)
        => StyleConsole.Default.WriteLine(new JsonConsoleStyle().CreateTextCollection(json, 0));

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="json">The JSON instance.</param>
    public static void WriteLine(JsonConsoleStyle style, IJsonValueNode json)
        => StyleConsole.Default.WriteLine((style ?? new JsonConsoleStyle()).CreateTextCollection(json, 0));

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="json">The JSON instance.</param>
    public static void WriteLine(System.Text.Json.Nodes.JsonObject json)
        => StyleConsole.Default.WriteLine(new JsonConsoleStyle().CreateTextCollection(json == null ? null : (JsonObjectNode)json, 0));

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="json">The JSON instance.</param>
    public static void WriteLine(JsonConsoleStyle style, System.Text.Json.Nodes.JsonObject json)
        => StyleConsole.Default.WriteLine((style ?? new JsonConsoleStyle()).CreateTextCollection(json == null ? null : (JsonObjectNode)json, 0));

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="json">The JSON instance.</param>
    public static void WriteLine(IJsonObjectHost json)
        => StyleConsole.Default.WriteLine(new JsonConsoleStyle().CreateTextCollection(json?.ToJson(), 0));

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="json">The JSON instance.</param>
    public static void WriteLine(JsonConsoleStyle style, IJsonObjectHost json)
        => StyleConsole.Default.WriteLine((style ?? new JsonConsoleStyle()).CreateTextCollection(json?.ToJson(), 0));

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="json">The JSON instance.</param>
    public static void WriteLine(System.Text.Json.Nodes.JsonArray json)
        => StyleConsole.Default.WriteLine(new JsonConsoleStyle().CreateTextCollection(json == null ? null : (JsonArrayNode)json, 0));

    /// <summary>
    /// Writes a JSON object, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="json">The JSON instance.</param>
    public static void WriteLine(JsonConsoleStyle style, System.Text.Json.Nodes.JsonArray json)
        => StyleConsole.Default.WriteLine((style ?? new JsonConsoleStyle()).CreateTextCollection(json == null ? null : (JsonArrayNode)json, 0));

    /// <summary>
    /// Writes the specified data, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <param name="model">A representation model.</param>
    public static void WriteLine<T>(IConsoleTextCreator model)
        => StyleConsole.Default.WriteLine(model);

    /// <summary>
    /// Writes the specified data, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <typeparam name="T">The type of data model.</typeparam>
    /// <param name="style">The style.</param>
    /// <param name="data">A data model.</param>
    public static void WriteLine<T>(IConsoleTextCreator<T> style, T data)
        => StyleConsole.Default.WriteLine(style, data);

    /// <summary>
    /// Writes the specified data, followed by the current line terminator, to the standard output stream.
    /// It will flush immediately.
    /// </summary>
    /// <typeparam name="TData">The type of data model.</typeparam>
    /// <typeparam name="TOptions">The additional options.</typeparam>
    /// <param name="style">The style.</param>
    /// <param name="data">A data model.</param>
    /// <param name="options">The additional options.</param>
    public static void WriteLine<TData, TOptions>(IConsoleTextCreator<TData, TOptions> style, TData data, TOptions options)
        => StyleConsole.Default.WriteLine(style, data, options);

    /// <summary>
    /// Writes a progress component, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="style">The options.</param>
    /// <returns>The progress result.</returns>
    public static OneProgress WriteLine(ConsoleProgressStyle style)
        => StyleConsole.Default.WriteLine(style, null);

    /// <summary>
    /// Writes a progress component, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="caption">The caption; or null if no caption. It will be better if it is less than 20 characters.</param>
    /// <param name="progressSize">The progress size.</param>
    /// <param name="kind">The progress kind.</param>
    /// <returns>The progress result.</returns>
    public static OneProgress WriteLine(ConsoleProgressStyle.Sizes progressSize, string caption, ConsoleProgressStyle.Kinds kind = ConsoleProgressStyle.Kinds.Full)
        => StyleConsole.Default.WriteLine(progressSize, caption, kind);

    /// <summary>
    /// Writes a progress component, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="progressSize">The progress size.</param>
    /// <param name="kind">The progress kind.</param>
    /// <returns>The progress result.</returns>
    public static OneProgress WriteLine(ConsoleProgressStyle.Sizes progressSize, ConsoleProgressStyle.Kinds kind = ConsoleProgressStyle.Kinds.Full)
        => StyleConsole.Default.WriteLine(progressSize, kind);

    /// <summary>
    /// Writes a progress component, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="caption">The caption; or null if no caption. It will be better if it is less than 20 characters.</param>
    /// <param name="style">The style.</param>
    /// <returns>The progress result.</returns>
    public static OneProgress WriteLine(ConsoleProgressStyle style, string caption)
        => StyleConsole.Default.WriteLine(style, caption);

    /// <summary>
    /// Writes the specific lines to the standard output stream.
    /// </summary>
    /// <param name="count">The count of line.</param>
    public static void WriteLines(int count)
        => StyleConsole.Default.WriteLines(count);

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="content">The text content collection.</param>
    public static void WriteLines(IEnumerable<ConsoleText> content)
        => StyleConsole.Default.WriteLines(content);

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="content">The text content.</param>
    /// <param name="additionalContext">The additional text content collection.</param>
    public static void WriteLines(ConsoleText content, params ConsoleText[] additionalContext)
        => StyleConsole.Default.WriteLines(content, additionalContext);

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="col">The string collection to write. Each one in a line.</param>
    public static void WriteLines(IEnumerable<string> col)
        => StyleConsole.Default.WriteLines(col);

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="col">The string collection to write. Each one in a line.</param>
    public static void WriteLines(ConsoleTextStyle style, IEnumerable<string> col)
        => StyleConsole.Default.WriteLines(style, col);

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="foreground">The foreground color of the console.</param>
    /// <param name="col">The string collection to write. Each one in a line.</param>
    public static void WriteLines(ConsoleColor foreground, IEnumerable<string> col)
        => StyleConsole.Default.WriteLines(foreground, col);

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="foreground">The foreground color of the console.</param>
    /// <param name="background">The background color.</param>
    /// <param name="col">The string collection to write. Each one in a line.</param>
    public static void WriteLines(ConsoleColor foreground, ConsoleColor background, IEnumerable<string> col)
        => StyleConsole.Default.WriteLines(foreground, background, col);

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="col">The string collection to write. Each one in a line.</param>
    /// <param name="selector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
    public static void WriteLines<T>(IEnumerable<T> col, Func<T, int, string> selector)
    {
        if (col == null) return;
        selector ??= (ele, i) => ele?.ToString();
        StyleConsole.Default.WriteLines(col.Select(selector));
    }

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="foreground">The foreground color of the console.</param>
    /// <param name="col">The string collection to write. Each one in a line.</param>
    /// <param name="selector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
    public static void WriteLines<T>(ConsoleColor foreground, IEnumerable<T> col, Func<T, int, string> selector)
    {
        if (col == null) return;
        selector ??= (ele, i) => ele?.ToString();
        StyleConsole.Default.WriteLines(foreground, col.Select(selector));
    }

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="col">The string collection to write. Each one in a line.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    public static void WriteLines<T>(IEnumerable<T> col, Func<T, string> selector)
    {
        if (col == null) return;
        selector ??= ele => ele?.ToString();
        StyleConsole.Default.WriteLines(col.Select(selector));
    }

    /// <summary>
    /// Writes the current line terminator for each item, to the standard output stream.
    /// </summary>
    /// <param name="foreground">The foreground color of the console.</param>
    /// <param name="col">The string collection to write. Each one in a line.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    public static void WriteLines<T>(ConsoleColor foreground, IEnumerable<T> col, Func<T, string> selector)
    {
        if (col == null) return;
        selector ??= ele => ele?.ToString();
        StyleConsole.Default.WriteLines(foreground, col.Select(selector));
    }
}
