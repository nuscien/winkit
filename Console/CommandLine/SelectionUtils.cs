using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security;

using Trivial.Collection;

namespace Trivial.CommandLine;

/// <summary>
/// The extensions for console renderer.
/// </summary>
public static partial class ConsoleRenderExtensions
{
    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="cli">The command line interface proxy.</param>
    /// <param name="collection">The collection data.</param>
    /// <param name="convert">The converter.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<T> Select<T>(this StyleConsole cli, IEnumerable<T> collection, Func<T, SelectionItem<T>> convert, SelectionConsoleOptions options = null)
    {
        var c = new SelectionData<T>();
        c.AddRange(collection.Select(convert));
        return Select(cli, c, options);
    }

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="cli">The command line interface proxy.</param>
    /// <param name="path">The parent foler path.</param>
    /// <param name="options">The selection display options.</param>
    /// <param name="searchPattern">The search string to match against the names of directories and files. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>The result of selection.</returns>
    /// <exception cref="ArgumentException">searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars method.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    public static SelectionResult<FileSystemInfo> Select(this StyleConsole cli, DirectoryInfo path, SelectionConsoleOptions options = null, string searchPattern = null)
    {
        var c = new SelectionData<FileSystemInfo>();
        var col = string.IsNullOrEmpty(searchPattern) ? path.GetFileSystemInfos() : path.GetFileSystemInfos(searchPattern);
        foreach (var f in col)
        {
            c.Add(f.Name, f);
        }

        return Select(cli, c, options);
    }

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="cli">The command line interface proxy.</param>
    /// <param name="path">The parent foler path.</param>
    /// <param name="onlyFiles">true if only display files; otherwise, false.</param>
    /// <param name="options">The selection display options.</param>
    /// <param name="searchPattern">The search string to match against the names of directories and files. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>The result of selection.</returns>
    /// <exception cref="ArgumentException">searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars method.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    public static SelectionResult<FileSystemInfo> Select(this StyleConsole cli, DirectoryInfo path, bool onlyFiles, SelectionConsoleOptions options = null, string searchPattern = null)
    {
        if (!onlyFiles) return Select(cli, path, options, searchPattern);
        var c = new SelectionData<FileSystemInfo>();
        var col = string.IsNullOrEmpty(searchPattern) ? path.GetFiles() : path.GetFiles(searchPattern);
        foreach (var f in col)
        {
            c.Add(f.Name, f);
        }

        return Select(cli, c, options);
    }

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="cli">The command line interface proxy.</param>
    /// <param name="path">The parent foler path.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    public static SelectionResult<FileSystemInfo> Select(this StyleConsole cli, DirectoryInfo path, Func<FileSystemInfo, bool> predicate, SelectionConsoleOptions options = null)
    {
        var c = new SelectionData<FileSystemInfo>();
        IEnumerable<FileSystemInfo> col = path.GetFileSystemInfos();
        if (predicate != null) col = col.Where(predicate);
        foreach (var f in col)
        {
            c.Add(f.Name, f);
        }

        return Select(cli, c, options);
    }

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="cli">The command line interface proxy.</param>
    /// <param name="collection">The collection data.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<string> Select(this StyleConsole cli, IEnumerable<string> collection, SelectionConsoleOptions options = null)
    {
        if (collection is null) return new(string.Empty, SelectionResultTypes.Canceled);
        var c = new SelectionData<string>();
        c.AddRange(collection);
        return Select(cli, c, options);
    }

    /// <summary>
    /// Tests if the input string is to get help.
    /// </summary>
    /// <param name="s">The input string.</param>
    /// <returns>true if to get help; otherwise, false.</returns>
    public static bool IsAboutToGetHelp(string s)
        => !string.IsNullOrEmpty(s) && s.Trim().ToLowerInvariant() switch
        {
            "?" or "help" or "gethelp" or "get-help" or "-?" or "/h" or "--?" or "-help" or "--help" or "/help" or "帮助" or "bangzhu" or "/bangzhu" or "--bangzhu" or "获取帮助" or "助け" or "❓" => true,
            _ => false
        };

    /// <summary>
    /// Tests if the input string is to exit.
    /// </summary>
    /// <param name="s">The input string.</param>
    /// <returns>true if to exit; otherwise, false.</returns>
    public static bool IsAboutToExit(string s)
        => !string.IsNullOrEmpty(s) && s.Trim().ToLowerInvariant() switch
        {
            "exit" or "quit" or "close" or "bye" or "byebye" or "goodbye" or "good-bye" or "end" or "shutdown" or "shut-down" or "关闭" or "退出" or "结束" or "再见" or "guanbi" or "tuichu" or "jieshu" or "zaijian" or "さようなら" => true,
            _ => false
        };

    private static int GetLetterWidth(char c)
    {
        if (c < 0x2E80) return 1;
        return c < 0xA500 || (c >= 0xF900 && c < 0xFB00) || (c >= 0xFE30 && c < 0xFE70)
            ? 2
            : 1;
    }
}
