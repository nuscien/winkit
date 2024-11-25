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
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="collection">The collection data.</param>
    /// <param name="convert">The converter.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<T> Select<T>(IEnumerable<T> collection, Func<T, SelectionItem<T>> convert, SelectionConsoleOptions options = null)
        => StyleConsole.Default.Select(collection, convert, options);

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="collection">The collection data.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<T> Select<T>(IEnumerable<SelectionItem<T>> collection, SelectionConsoleOptions options = null)
        => StyleConsole.Default.Select(collection, options);

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="path">The parent foler path.</param>
    /// <param name="options">The selection display options.</param>
    /// <param name="searchPattern">The search string to match against the names of directories and files. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>The result of selection.</returns>
    /// <exception cref="ArgumentException">searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars method.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    public static SelectionResult<FileSystemInfo> Select(DirectoryInfo path, SelectionConsoleOptions options = null, string searchPattern = null)
        => StyleConsole.Default.Select(path, options, searchPattern);

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="path">The parent foler path.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    public static SelectionResult<FileSystemInfo> Select(DirectoryInfo path, Func<FileSystemInfo, bool> predicate, SelectionConsoleOptions options = null)
        => StyleConsole.Default.Select(path, predicate, options);

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="collection">The collection data.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<T> Select<T>(SelectionData<T> collection, SelectionConsoleOptions options = null)
        => StyleConsole.Default.Select(collection, options);

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="collection">The collection data.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<string> Select(IEnumerable<string> collection, SelectionConsoleOptions options = null)
        => StyleConsole.Default.Select(collection, options);

    /// <summary>
    /// Flushes all data.
    /// </summary>
    public static void Flush()
        => StyleConsole.Default.Flush();

    /// <summary>
    /// Clears output cache.
    /// </summary>
    public static void ClearOutputCache()
        => StyleConsole.Default.ClearOutputCache();

    /// <summary>
    /// Enters a backspace to console to remove the last charactor.
    /// </summary>
    /// <param name="count">The count of the charactor to remove from end.</param>
    /// <param name="doNotRemoveOutput">true if just only move cursor back and keep output; otherwise, false.</param>
    public static void Backspace(int count = 1, bool doNotRemoveOutput = false)
        => StyleConsole.Default.Backspace(count, doNotRemoveOutput);

    /// <summary>
    /// Enters backspaces to console to remove the charactors to the beginning of the line.
    /// </summary>
    public static void BackspaceToBeginning()
        => StyleConsole.Default.BackspaceToBeginning();

    /// <summary>
    /// Reads the next line of characters from the standard input stream.
    /// </summary>
    /// <returns>The next line of characters from the input stream, or null if no more lines are available.</returns>
    /// <exception cref="IOException">An I/O error occurred.</exception>
    /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The number of characters in the next line of characters is greater than max value of 32-bit integer.</exception>
    public static string ReadLine()
        => StyleConsole.Default.ReadLine();

    /// <summary>
    /// Obtains the next character or function key pressed by the user. The pressed key is optionally displayed in the console window.
    /// </summary>
    /// <param name="intercept">Determines whether to display the pressed key in the console window. true to not display the pressed key; otherwise, false.</param>
    /// <returns>The next line of characters from the input stream, or null if no more lines are available.</returns>
    /// <exception cref="IOException">An I/O error occurred.</exception>
    /// <exception cref="InvalidOperationException">The input stream is redirected from the one other than the console.</exception>
    public static ConsoleKeyInfo ReadKey(bool intercept = false)
        => StyleConsole.Default.ReadKey(intercept);

    /// <summary>
    /// Obtains the password pressed by the user.
    /// </summary>
    /// <returns>
    /// The password.
    /// </returns>
    public static SecureString ReadPassword()
        => StyleConsole.Default.ReadPassword(null, null);

    /// <summary>
    /// Obtains the password pressed by the user.
    /// </summary>
    /// <param name="replaceChar">The optional charactor to output to replace the original one, such as *.</param>
    /// <param name="inline">true if do not follow the line terminator after typing the password; otherwise, false.</param>
    /// <returns>
    /// The password.
    /// </returns>
    public static SecureString ReadPassword(char replaceChar, bool inline = false)
        => StyleConsole.Default.ReadPassword(null, replaceChar, inline);

    /// <summary>
    /// Obtains the password pressed by the user.
    /// </summary>
    /// <param name="foreground">The replace charactor color.</param>
    /// <param name="replaceChar">The optional charactor to output to replace the original one, such as *.</param>
    /// <param name="inline">true if do not follow the line terminator after typing the password; otherwise, false.</param>
    /// <returns>
    /// The password.
    /// </returns>
    public static SecureString ReadPassword(ConsoleColor? foreground, char? replaceChar, bool inline = false)
        => StyleConsole.Default.ReadPassword(foreground, replaceChar, inline);

    /// <summary>
    /// Moves cursor by a specific relative position.
    /// </summary>
    /// <param name="x">The horizontal translation size.</param>
    /// <param name="y">The vertical translation size.</param>
    public static void MoveCursorBy(int x, int y = 0)
        => StyleConsole.Default.MoveCursorBy(x, y);

    /// <summary>
    /// Moves cursor at a specific position in buffer.
    /// </summary>
    /// <param name="x">Column, the left from the edge of buffer.</param>
    /// <param name="y">Row, the top from the edge of buffer.</param>
    public static void MoveCursorTo(int x, int y)
        => StyleConsole.Default.MoveCursorTo(x, y);

    /// <summary>
    /// Removes the specific area.
    /// </summary>
    /// <param name="area">The area to remove.</param>
    public static void Clear(StyleConsole.RelativeAreas area)
        => StyleConsole.Default.Clear(area);
}
