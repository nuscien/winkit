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
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="content">The text content.</param>
    public static void Write(ConsoleText content)
        => StyleConsole.Default.Write(content);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="content1">The text content 1.</param>
    /// <param name="content2">The text content 2.</param>
    /// <param name="additionalContext">The additional text content collection.</param>
    public static void Write(ConsoleText content1, ConsoleText content2, params ConsoleText[] additionalContext)
        => StyleConsole.Default.Write(content1, content2, additionalContext);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="content">The text content collection.</param>
    public static void Write(IEnumerable<ConsoleText> content)
        => StyleConsole.Default.Write(content);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="content">The text content collection.</param>
    public static void Write(ReadOnlySpan<ConsoleText> content)
        => StyleConsole.Default.Write(content);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(string s, params object[] args)
        => StyleConsole.Default.Write(s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(ConsoleTextStyle style, string s, params object[] args)
        => StyleConsole.Default.Write(style, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(ConsoleColor foreground, string s, params object[] args)
        => StyleConsole.Default.Write(foreground, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, string s, params object[] args)
        => StyleConsole.Default.Write(foreground, background, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(Color foreground, string s, params object[] args)
        => StyleConsole.Default.Write(foreground, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(Color foreground, Color background, string s, params object[] args)
        => StyleConsole.Default.Write(foreground, background, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(IConsoleTextPrettier style, string s, params object[] args)
        => StyleConsole.Default.Write(style, s, args);

#if NET9_0_OR_GREATER

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(string s, params ReadOnlySpan<object> args)
        => StyleConsole.Default.Write(s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(ConsoleTextStyle style, string s, params ReadOnlySpan<object> args)
        => StyleConsole.Default.Write(style, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(ConsoleColor foreground, string s, params ReadOnlySpan<object> args)
        => StyleConsole.Default.Write(foreground, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, string s, params ReadOnlySpan<object> args)
        => StyleConsole.Default.Write(foreground, background, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(Color foreground, string s, params ReadOnlySpan<object> args)
        => StyleConsole.Default.Write(foreground, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(Color foreground, Color background, string s, params ReadOnlySpan<object> args)
        => StyleConsole.Default.Write(foreground, background, s, args);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="s">A composite format string to output.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <exception cref="FormatException">format is invalid. -or- The index of a format item is less than zero, or greater than or equal to the length of the args array.</exception>
    public static void Write(IConsoleTextPrettier style, string s, params ReadOnlySpan<object> args)
        => StyleConsole.Default.Write(style, s, args);
#endif

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(StringBuilder s)
        => StyleConsole.Default.Write(s);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(ConsoleTextStyle style, StringBuilder s)
        => StyleConsole.Default.Write(style, s);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(ConsoleColor foreground, StringBuilder s)
        => StyleConsole.Default.Write(foreground, s);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, StringBuilder s)
        => StyleConsole.Default.Write(foreground, background, s);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(Color foreground, StringBuilder s)
        => StyleConsole.Default.Write(foreground, s);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(Color foreground, Color background, StringBuilder s)
        => StyleConsole.Default.Write(foreground, background, s);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(IConsoleTextPrettier style, StringBuilder s)
        => StyleConsole.Default.Write(style, s);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(SecureString s)
        => StyleConsole.Default.Write(s.ToUnsecureString());

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(ConsoleTextStyle style, SecureString s)
        => StyleConsole.Default.Write(style, s.ToUnsecureString());

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(ConsoleColor foreground, SecureString s)
        => StyleConsole.Default.Write(foreground, s.ToUnsecureString());

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="s">A composite format string to output.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, SecureString s)
        => StyleConsole.Default.Write(foreground, background, s.ToUnsecureString());

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void Write(int number)
        => StyleConsole.Default.Write(number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(int number, string format)
        => StyleConsole.Default.Write(number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleTextStyle style, int number)
        => StyleConsole.Default.Write(style, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleTextStyle style, int number, string format)
        => StyleConsole.Default.Write(style, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor foreground, int number)
        => StyleConsole.Default.Write(foreground, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleColor foreground, int number, string format)
        => StyleConsole.Default.Write(foreground, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, int number)
        => StyleConsole.Default.Write(foreground, background, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, int number, string format)
        => StyleConsole.Default.Write(foreground, background, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void Write(long number)
        => StyleConsole.Default.Write(number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(long number, string format)
        => StyleConsole.Default.Write(number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleTextStyle style, long number)
        => StyleConsole.Default.Write(style, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleTextStyle style, long number, string format)
        => StyleConsole.Default.Write(style, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor foreground, long number)
        => StyleConsole.Default.Write(foreground, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleColor foreground, long number, string format)
        => StyleConsole.Default.Write(foreground, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, long number)
        => StyleConsole.Default.Write(foreground, background, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, long number, string format)
        => StyleConsole.Default.Write(foreground, background, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void Write(ulong number)
        => StyleConsole.Default.Write(number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleTextStyle style, ulong number)
        => StyleConsole.Default.Write(style, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor foreground, ulong number)
        => StyleConsole.Default.Write(foreground, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, ulong number)
        => StyleConsole.Default.Write(foreground, background, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void Write(float number)
        => StyleConsole.Default.Write(number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(float number, string format)
        => StyleConsole.Default.Write(number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleTextStyle style, float number)
        => StyleConsole.Default.Write(style, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleTextStyle style, float number, string format)
        => StyleConsole.Default.Write(style, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor foreground, float number)
        => StyleConsole.Default.Write(foreground, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleColor foreground, float number, string format)
        => StyleConsole.Default.Write(foreground, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, float number)
        => StyleConsole.Default.Write(foreground, background, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, float number, string format)
        => StyleConsole.Default.Write(foreground, background, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void Write(decimal number)
        => StyleConsole.Default.Write(number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleTextStyle style, decimal number)
        => StyleConsole.Default.Write(style, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor foreground, decimal number)
        => StyleConsole.Default.Write(foreground, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, decimal number)
        => StyleConsole.Default.Write(foreground, background, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    public static void Write(double number)
        => StyleConsole.Default.Write(number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(double number, string format)
        => StyleConsole.Default.Write(number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleTextStyle style, double number)
        => StyleConsole.Default.Write(style, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleTextStyle style, double number, string format)
        => StyleConsole.Default.Write(style, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor foreground, double number)
        => StyleConsole.Default.Write(foreground, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleColor foreground, double number, string format)
        => StyleConsole.Default.Write(foreground, number, format);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, double number)
        => StyleConsole.Default.Write(foreground, background, number);

    /// <summary>
    /// Writes the specified number to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="number">A number to output.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <exception cref="FormatException">format is invalid or not supported.</exception>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, double number, string format)
        => StyleConsole.Default.Write(foreground, background, number, format);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void Write(char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.Write(value, start, count);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void Write(ConsoleTextStyle style, char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.Write(style, value, start, count);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void Write(ConsoleColor foreground, char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.Write(foreground, value, start, count);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.Write(foreground, background, value, start, count);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="count">The number of characters to write.</param>
    public static void Write(IConsoleTextPrettier style, char[] value, int start = 0, int? count = null)
        => StyleConsole.Default.Write(style, value, start, count);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void Write(char value, int repeatCount = 1)
        => StyleConsole.Default.Write(value, repeatCount);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The content style.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void Write(ConsoleTextStyle style, char value, int repeatCount = 1)
        => StyleConsole.Default.Write(style, value, repeatCount);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void Write(ConsoleColor foreground, char value, int repeatCount = 1)
        => StyleConsole.Default.Write(foreground, value, repeatCount);

    /// <summary>
    /// Writes the specified string value to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void Write(IConsoleTextPrettier style, char value, int repeatCount = 1)
        => StyleConsole.Default.Write(style, value, repeatCount);

    /// <summary>
    /// Writes the specified data to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="model">A representation model.</param>
    public static void Write(IConsoleTextCreator model)
        => StyleConsole.Default.Write(model);

    /// <summary>
    /// Writes the specified data to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <typeparam name="T">The type of data model.</typeparam>
    /// <param name="style">The style.</param>
    /// <param name="data">A data model.</param>
    public static void Write<T>(IConsoleTextCreator<T> style, T data)
        => StyleConsole.Default.Write(style, data);

    /// <summary>
    /// Writes the specified data to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <typeparam name="TData">The type of data model.</typeparam>
    /// <typeparam name="TOptions">The additional options.</typeparam>
    /// <param name="style">The style.</param>
    /// <param name="data">A data model.</param>
    /// <param name="options">The additional options.</param>
    public static void Write<TData, TOptions>(IConsoleTextCreator<TData, TOptions> style, TData data, TOptions options)
        => StyleConsole.Default.Write(style, data, options);

    /// <summary>
    /// Writes the specified characters to the standard output stream.
    /// Note it may not flush immediately.
    /// </summary>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    public static void Write(ConsoleColor? foreground, ConsoleColor? background, char value, int repeatCount = 1)
        => StyleConsole.Default.Write(foreground, background, value, repeatCount);
}
