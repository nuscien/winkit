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

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="cli">The command line interface proxy.</param>
    /// <param name="collection">The collection data.</param>
    /// <param name="options">The selection display options.</param>
    /// <param name="select">The index of item selected.</param>
    /// <returns>The result of selection: offset, count, rows, columns, paging tips, customized tips, page size, item length.</returns>
    private static (int, int, int, int, bool, bool, int, int) RenderData<T>(this StyleConsole cli, List<SelectionItem<T>> collection, SelectionConsoleOptions options, int select)
    {
        var maxWidth = GetBufferSafeWidth(cli);
        var itemLen = options.Column.HasValue ? (int)Math.Floor(maxWidth * 1.0 / options.Column.Value) : maxWidth;
        if (options.MaxLength.HasValue) itemLen = Math.Min(options.MaxLength.Value, itemLen);
        if (options.MinLength.HasValue) itemLen = Math.Max(options.MinLength.Value, itemLen);
        if (itemLen > maxWidth) itemLen = maxWidth;
        var columns = (int)Math.Floor(maxWidth * 1.0 / itemLen);
        if (options.Column.HasValue && columns > options.Column.Value) columns = options.Column.Value;
        var maxRows = 50;
        try
        {
            maxRows = Console.BufferHeight - 5;
            if (maxRows < 1) maxRows = 50;
        }
        catch (InvalidOperationException)
        {
        }
        catch (IOException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (NotSupportedException)
        {
        }

        if (options.MaxRow.HasValue && options.MaxRow.Value < maxRows)
            maxRows = options.MaxRow.Value;
        var pageSize = columns * maxRows;
        var needPaging = collection.Count > pageSize;
        if (select >= collection.Count) select = collection.Count - 1;
        var list = collection;
        var offset = 0;
        if (select >= pageSize)
        {
            offset = (int)Math.Floor(select * 1.0 / pageSize) * pageSize;
            list = list.Skip(offset).Take(pageSize).ToList();
        }
        else if (needPaging)
        {
            list = list.Take(pageSize).ToList();
        }

        var i = offset;
        var lastColIndex = columns - 1;
        var rows = -1;
        SelectionItem<T> selItem = null;
        foreach (var item in list)
        {
            if (string.IsNullOrEmpty(item.Title)) continue;
            var isSel = i == select;
            if (isSel) selItem = item;
            RenderData(cli, item, options, isSel, itemLen);
            var indexInRow = i % columns;
            if (indexInRow == lastColIndex)
                cli.Append(Environment.NewLine);
            else if (indexInRow == 0)
                rows++;
            i++;
        }

        if (list.Count % columns > 0) cli.Append(Environment.NewLine);
        var hasPagingTips = false;
        var tipsP = options.PagingTips;
        if (needPaging && !string.IsNullOrEmpty(tipsP))
        {
            cli.Append(
                new ConsoleTextStyle(
                    options.PagingForegroundRgbColor,
                    options.PagingForegroundConsoleColor ?? options.ForegroundColor,
                    options.PagingBackgroundRgbColor,
                    options.PagingBackgroundConsoleColor ?? options.BackgroundColor),
                tipsP
                    .Replace("{from}", (offset + 1).ToString())
                    .Replace("{end}", (offset + list.Count).ToString())
                    .Replace("{count}", list.Count.ToString("g"))
                    .Replace("{size}", pageSize.ToString("g"))
                    .Replace("{total}", collection.Count.ToString("g")));
            cli.Append(Environment.NewLine);
            hasPagingTips = true;
        }

        var hasTips = false;
        if (!string.IsNullOrEmpty(options.Tips))
        {
            cli.Append(
                new ConsoleTextStyle(
                    options.TipsForegroundRgbColor,
                    options.TipsForegroundConsoleColor ?? options.ForegroundColor,
                    options.TipsBackgroundRgbColor,
                    options.TipsBackgroundConsoleColor ?? options.BackgroundColor),
                options.Tips.Length < maxWidth - 1
                    ? options.Tips
                    : (options.Tips.Substring(0, maxWidth - 5) + "..."));
            cli.Append(Environment.NewLine);
            hasTips = true;
        }

        RenderSelectResult(cli, selItem?.Title, options);
        return (offset, list.Count, rows, columns, hasPagingTips, hasTips, pageSize, itemLen);
    }

    private static void RenderSelectResult(StyleConsole cli, string value, SelectionConsoleOptions options)
    {
        cli.Append(
            new ConsoleTextStyle(
                options.QuestionForegroundRgbColor,
                options.QuestionForegroundConsoleColor ?? options.ForegroundColor,
                options.QuestionBackgroundRgbColor,
                options.QuestionBackgroundConsoleColor ?? options.BackgroundColor),
            options.Question);
        if (!string.IsNullOrWhiteSpace(value))
            cli.Append(options.ForegroundColor, options.BackgroundColor, value);
        else
            cli.Flush();
    }

    private static void RenderData<T>(StyleConsole cli, SelectionItem<T> item, SelectionConsoleOptions options, bool isSelect, int len)
    {
        var style = isSelect ? new ConsoleTextStyle(
            options.SelectedForegroundRgbColor,
            options.SelectedForegroundConsoleColor ?? options.ForegroundColor,
            options.SelectedBackgroundRgbColor,
            options.SelectedBackgroundConsoleColor ?? options.BackgroundColor) : new ConsoleTextStyle(
            options.ItemForegroundRgbColor,
            options.ItemForegroundConsoleColor ?? options.ForegroundColor,
            options.ItemBackgroundRgbColor,
            options.ItemBackgroundConsoleColor ?? options.BackgroundColor);
        var sb = new StringBuilder();
        var j = 0;
        var maxLen = len - 1;
        var curLeft = TryGetCursorLeft(cli) ?? -1;
        foreach (var c in (isSelect ? options.SelectedPrefix : options.Prefix) ?? string.Empty)
        {
            var c2 = c;
            switch (c)
            {
                case '\t':
                case '\r':
                case '\n':
                    j++;
                    c2 = ' ';
                    break;
                case '\0':
                case '\b':
                    continue;
                default:
                    j += GetLetterWidth(c);
                    break;
            }

            if (j >= maxLen) break;
            sb.Append(c2);
        }

        foreach (var c in item.Title)
        {
            var c2 = c;
            switch (c)
            {
                case '\t':
                case '\r':
                case '\n':
                    j++;
                    c2 = ' ';
                    break;
                case '\0':
                case '\b':
                    continue;
                default:
                    j += GetLetterWidth(c);
                    break;
            }

            if (j >= maxLen) break;
            sb.Append(c2);
        }

        if (curLeft >= 0)
        {
            cli.Append(style, sb);
            var rest = curLeft + len - cli.CursorLeft;
            if (rest > 0)
                cli.Append(style, ' ', rest);
            else if (rest < 0)
                cli.Append(
                    new ConsoleTextStyle(
                        options.ItemForegroundRgbColor,
                        options.ItemForegroundConsoleColor ?? options.ForegroundColor,
                        options.ItemBackgroundRgbColor,
                        options.ItemBackgroundConsoleColor ?? options.BackgroundColor),
                    " \b");
        }
        else
        {
            sb.Append(' ', len - j);
            cli.Append(style, sb);
        }

        try
        {
            if (curLeft >= 0)
            {
                curLeft += len;
                var rest = curLeft - cli.CursorLeft;
                if (rest < 0)
                {
                    cli.MoveCursorBy(rest, 0);
                    cli.Append(
                        new ConsoleTextStyle(
                        options.ItemForegroundRgbColor,
                        options.ItemForegroundConsoleColor ?? options.ForegroundColor,
                        options.ItemBackgroundRgbColor,
                        options.ItemBackgroundConsoleColor ?? options.BackgroundColor),
                        " \b");
                }
                else if (rest > 0)
                {
                    cli.MoveCursorBy(rest, 0);
                }
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (IOException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (NotSupportedException)
        {
        }
    }

    private static int GetLetterWidth(char c)
    {
        if (c < 0x2E80) return 1;
        return c < 0xA500 || (c >= 0xF900 && c < 0xFB00) || (c >= 0xFE30 && c < 0xFE70)
            ? 2
            : 1;
    }
}
