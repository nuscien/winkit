using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;

namespace Trivial.CommandLine;

public static partial class ConsoleRenderExtensions
{
    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <param name="console">The command line interface proxy.</param>
    /// <param name="dispatcher">The command dispatcher.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<string> Select(this StyleConsole console, CommandDispatcher dispatcher, SelectionConsoleOptions options = null)
    {
        var selection = new SelectionData<string>();
        var description = dispatcher.GetDescription();
        foreach (var item in description)
        {
            selection.Add($"{item.Key}\t{item.Value}".Trim(), item.Key);
        }

        return Select(console, selection, options ?? new()
        {
            Prefix = "· ",
            SelectedPrefix = "→ ",
            Tips = null,
            Question = null,
            SelectedForegroundConsoleColor = ConsoleColor.Yellow,
            SelectedForegroundRgbColor = Color.FromArgb(0x55, 0xCC, 0xEE),
            SelectedBackgroundConsoleColor = null,
            SelectedBackgroundRgbColor = null,
        });
    }

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="console">The command line interface proxy.</param>
    /// <param name="data">The collection data.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<T> Select<T>(this StyleConsole console, IEnumerable<SelectionItem<T>> data, SelectionConsoleOptions options = null)
    {
        var selection = new SelectionData<T>();
        selection.AddRange(data);
        return Select(console, selection, options);
    }

    /// <summary>
    /// Writes a collection of item for selecting.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="console">The command line interface proxy.</param>
    /// <param name="data">The collection data.</param>
    /// <param name="options">The selection display options.</param>
    /// <returns>The result of selection.</returns>
    public static SelectionResult<T> Select<T>(this StyleConsole console, SelectionData<T> data, SelectionConsoleOptions options = null)
    {
        if (data is null) return new(string.Empty, SelectionResultTypes.Canceled);
        console ??= StyleConsole.Default;
        options ??= new();
        if ((console.Mode != StyleConsole.Modes.Ansi && console.Mode != StyleConsole.Modes.Cmd && console.Handler == null) || !console.TryGetCursorTop().HasValue)
            return SelectInternal(console, data, options);
        console.Flush();
        var selected = -1;
        while (true)
        {
            var refreshWindow = false;
            if (selected < 0)
            {
                selected = 0;
            }

            var list = data.ToList();
            var count = list.Count;
            if (selected >= count)
            {
                if (count < 1) return new(string.Empty, SelectionResultTypes.Canceled);
                selected %= count;
            }

            var select = list[selected];
            var maxWidth = GetBufferSafeWidth(console);
            var maxHeight = GetBufferSafeHeight(console);
            var maxRows = options.MaxRow ?? 50;
            var hasTips = !string.IsNullOrWhiteSpace(options.Tips);
            var tipsHeight = hasTips ? 3 : 2;
            if (maxRows < 1) maxRows = 1;
            else if (maxRows > maxHeight) maxRows = maxHeight - tipsHeight;
            var columns = options.Column ?? 1;
            if (columns < 1) columns = 1;
            var itemWidth = maxWidth / columns;
            if (options.MinLength.HasValue && itemWidth < options.MinLength.Value)
            {
                columns = maxWidth / options.MinLength.Value;
                if (columns < 1) columns = 1;
            }

            if (options.MaxLength.HasValue && options.MaxLength.Value > 1 && itemWidth > options.MaxLength.Value)
                itemWidth = options.MaxLength.Value;
            var absolutePageSize = maxRows * columns;
            var isFullWindow = absolutePageSize >= count;
            if (isFullWindow)
            {
                maxRows = (int)Math.Ceiling(count * 1.0 / columns);
                absolutePageSize = maxRows * columns;
            }

            var pageSize = count;
            if (options.MaxRow.HasValue && !isFullWindow)
                pageSize = maxRows * columns;
            var start = selected >= pageSize ? (selected / pageSize * pageSize) : 0;
            var pos = 0;
            var len = absolutePageSize + start;
            var selectText = string.Empty;
            var singleColumn = columns == 1 && !options.Column.HasValue;
            for (var i = start; i < len; i++)
            {
                var item = i >= list.Count ? new(string.Empty) : list[i];
                var nextPos = pos + itemWidth;
                if (nextPos > maxWidth)
                {
                    pos = 0;
                    nextPos = itemWidth;
                    console.WriteLine();
                }

                var isSelect = i == selected;
                var title = item?.Title?.Trim() ?? string.Empty;
                if (isSelect) selectText = title;
                var prefix = isSelect ? options.SelectedPrefix : options.Prefix;
                if (!string.IsNullOrEmpty(prefix)) title = string.Concat(prefix, title);
                if (title.Length < 1)
                {
                    title = " ";
                }
                else if (singleColumn)
                {
                    var pos2 = title.IndexOf('\t');
                    if (pos2 > 0)
                    {
                        var s = title.Substring(0, pos2).TrimEnd();
                        title = string.Concat(s, s.Length > 7 ? " \t" : "\t\t", title.Substring(pos2 + 1).TrimStart());
                    }
                }

                RenderSentence(console, isSelect ? new()
                {
                    ForegroundConsoleColor = options.SelectedForegroundConsoleColor ?? options.ForegroundColor,
                    ForegroundRgbColor = options.SelectedForegroundRgbColor,
                    BackgroundConsoleColor = options.SelectedBackgroundConsoleColor ?? options.BackgroundColor,
                    BackgroundRgbColor = options.SelectedBackgroundRgbColor,
                } : new()
                {
                    ForegroundConsoleColor = options.ItemForegroundConsoleColor ?? options.ForegroundColor,
                    ForegroundRgbColor = options.ItemForegroundRgbColor,
                    BackgroundConsoleColor = options.ItemBackgroundConsoleColor ?? options.BackgroundColor,
                    BackgroundRgbColor = options.ItemBackgroundRgbColor,
                }, title, pos, itemWidth - 1, singleColumn, maxWidth);
                pos = nextPos;
            }

            console.WriteLine();
            var end = Math.Min(start + absolutePageSize, list.Count);
            if (string.IsNullOrWhiteSpace(options.PagingTips) || isFullWindow)
            {
                tipsHeight--;
            }
            else
            {
                RenderSentence(console, new()
                {
                    ForegroundConsoleColor = options.PagingForegroundConsoleColor ?? options.ForegroundColor,
                    ForegroundRgbColor = options.PagingForegroundRgbColor,
                    BackgroundConsoleColor = options.PagingBackgroundConsoleColor ?? options.BackgroundColor,
                    BackgroundRgbColor = options.PagingBackgroundRgbColor,
                }, options.PagingTips
                .Replace("{from}", (start + 1).ToString("g"))
                .Replace("{end}", end.ToString("g"))
                .Replace("{count}", (end - start).ToString("g"))
                .Replace("{size}", absolutePageSize.ToString("g"))
                .Replace("{total}", list.Count.ToString("g")), 0, maxWidth, true, maxWidth);
                console.WriteLine();
            }

            if (hasTips)
            {
                RenderSentence(console, new()
                {
                    ForegroundConsoleColor = options.TipsForegroundConsoleColor ?? options.ForegroundColor,
                    ForegroundRgbColor = options.TipsForegroundRgbColor,
                    BackgroundConsoleColor = options.TipsBackgroundConsoleColor ?? options.BackgroundColor,
                    BackgroundRgbColor = options.TipsBackgroundRgbColor,
                }, options.Tips, 0, maxWidth, true, maxWidth);
                console.WriteLine();
            }

            console.Clear(StyleConsole.RelativeAreas.Line);
            console.BackspaceToBeginning();
            if (options.Question is not null)
            {
                console.Write(new ConsoleTextStyle
                {
                    ForegroundConsoleColor = options.QuestionForegroundConsoleColor ?? options.ForegroundColor,
                    ForegroundRgbColor = options.QuestionForegroundRgbColor,
                    BackgroundConsoleColor = options.QuestionBackgroundConsoleColor ?? options.BackgroundColor,
                    BackgroundRgbColor = options.QuestionBackgroundRgbColor,
                }, options.Question);
                var pos2 = selectText.IndexOf('\t');
                if (pos2 > 0) selectText = selectText.Substring(0, pos2);
                console.Write(selectText);
            }

            var key = console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                case ConsoleKey.Select:
                case ConsoleKey.Spacebar:
                    console.WriteLine();
                    return new(select.Title, selected, select.Data, select.Title, SelectionResultTypes.Selected);
                case ConsoleKey.Backspace:
                case ConsoleKey.Delete:
                case ConsoleKey.Clear:
                case ConsoleKey.F4:
                    console.Clear(StyleConsole.RelativeAreas.Line);
                    console.BackspaceToBeginning();
                    console.Write(new ConsoleTextStyle
                    {
                        ForegroundConsoleColor = options.QuestionForegroundConsoleColor ?? options.ForegroundColor,
                        ForegroundRgbColor = options.QuestionForegroundRgbColor,
                        BackgroundConsoleColor = options.QuestionBackgroundConsoleColor ?? options.BackgroundColor,
                        BackgroundRgbColor = options.QuestionBackgroundRgbColor,
                    }, options.ManualQuestion ?? options.Question);
                    return SelectInternal(console, console.ReadLine(), data);
                case ConsoleKey.Escape:
                case ConsoleKey.Pause:
                case ConsoleKey.BrowserStop:
                    console.Clear(StyleConsole.RelativeAreas.Line);
                    console.BackspaceToBeginning();
                    return new SelectionResult<T>(string.Empty, SelectionResultTypes.Canceled);
                case ConsoleKey.Help:
                case ConsoleKey.F1:
                    {
                        console.Clear(StyleConsole.RelativeAreas.Line);
                        console.BackspaceToBeginning();
                        console.Write(new ConsoleTextStyle
                        {
                            ForegroundConsoleColor = options.QuestionForegroundConsoleColor ?? options.ForegroundColor,
                            ForegroundRgbColor = options.QuestionForegroundRgbColor,
                            BackgroundConsoleColor = options.QuestionBackgroundConsoleColor ?? options.BackgroundColor,
                            BackgroundRgbColor = options.QuestionBackgroundRgbColor,
                        }, options.ManualQuestion ?? options.Question);
                        console.WriteLine("?");
                        var item = data.Get('?', out var select2);
                        return item == null
                            ? new SelectionResult<T>("?", SelectionResultTypes.Selected)
                            : new SelectionResult<T>("?", select2, item.Data, item.Title);
                    }
                case ConsoleKey.BrowserRefresh:
                case ConsoleKey.F5:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                        selected = 0;
                    break;
                case ConsoleKey.F6:
                    console.Clear(StyleConsole.RelativeAreas.Line);
                    console.BackspaceToBeginning();
                    console.WriteLine("---");
                    refreshWindow = true;
                    break;
                case ConsoleKey.F12:
                    console.Clear(StyleConsole.RelativeAreas.Line);
                    console.BackspaceToBeginning();
                    console.WriteLine("---");
                    return SelectInternal(console, data, options);
                case ConsoleKey.PageUp:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                        selected = 0;
                    else
                        selected = Math.Min(start - pageSize, 0);
                    break;
                case ConsoleKey.PageDown:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                        selected = count - 1;
                    else
                        selected = Math.Min(start + pageSize, count - 1);
                    break;
                case ConsoleKey.UpArrow:
                    {
                        var i = selected - columns;
                        if (i < 0) i = count / columns * columns + (selected % columns);
                        if (i >= count) i -= columns;
                        selected = i;
                    }

                    break;
                case ConsoleKey.DownArrow:
                    {
                        var i = selected + columns;
                        if (i >= count) i = selected % columns;
                        selected = i;
                    }

                    break;
                case ConsoleKey.LeftArrow:
                    selected = selected < 1 ? (count - 1) : (selected - 1);
                    break;
                case ConsoleKey.RightArrow:
                    selected = (selected + 1) % count;
                    break;
                case ConsoleKey.Home:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                        selected = 0;
                    else
                        selected = start;
                    break;
                case ConsoleKey.End:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                        selected = count - 1;
                    else
                        selected = Math.Min(start + pageSize - 1, count - 1);
                    break;
                default:
                    {
                        var item = data.Get(key.KeyChar, out var selectIndex);
                        if (item is not null && selectIndex >= 0) selected = selectIndex;
                        break;
                    }
            }

            var currentTop = console.TryGetCursorTop();
            if (currentTop.HasValue && !refreshWindow) console.MoveCursorTo(0, currentTop.Value - maxRows - tipsHeight + 1);
        }
    }

    public static async Task ProcessOrSelectAsync(this CommandDispatcher dispatcher, bool skipSelectionTips, CancellationToken cancellationToken = default)
    {
        if (dispatcher is null) return;
        var console = StyleConsole.Default;
        if (console.Mode != StyleConsole.Modes.Ansi && console.Mode != StyleConsole.Modes.Cmd && console.Handler == null)
        {
            await dispatcher.ProcessAsync(cancellationToken);
            return;
        }

        string cmd = null;
        await dispatcher.ProcessAsync(() =>
        {
            var toSelect = Resource.ToSelect?.Trim();
            if (!skipSelectionTips && !string.IsNullOrEmpty(toSelect))
            {
                if (toSelect.EndsWith(": ")) toSelect = toSelect.Substring(0, toSelect.Length - 2).TrimEnd();
                else if (toSelect.EndsWith(":") || toSelect.EndsWith("：")) toSelect = toSelect.Substring(0, toSelect.Length - 1).TrimEnd();
                if (!string.IsNullOrEmpty(toSelect)) console.WriteLine(ConsoleColor.DarkGray, toSelect);
            }

            var result = Select(console, dispatcher);
            var arg = result.Data ?? result.Value;
            if (string.IsNullOrWhiteSpace(arg)) return true;
            cmd = arg;
            return true;
        }, cancellationToken);
        if (!string.IsNullOrEmpty(cmd)) await dispatcher.ProcessAsync(cmd, cancellationToken);
    }

    public static Task ProcessOrSelectAsync(this CommandDispatcher dispatcher, CancellationToken cancellationToken = default)
        => ProcessOrSelectAsync(dispatcher, false, cancellationToken);

    internal static void RenderSentence(StyleConsole console, ConsoleTextStyle style, string value, int start, int length)
        => RenderSentence(console, style, value, start, length, false, GetBufferSafeWidth(console));

    private static SelectionResult<T> SelectInternal<T>(StyleConsole console, SelectionData<T> data, SelectionConsoleOptions options = null)
    {
        var list = data.ToList();
        var style = options.FallbackStyle ? new ConsoleTextStyle
        {
            ForegroundConsoleColor = options.ItemForegroundConsoleColor ?? options.ForegroundColor,
            ForegroundRgbColor = options.ItemForegroundRgbColor,
            BackgroundConsoleColor = options.ItemBackgroundConsoleColor ?? options.BackgroundColor,
            BackgroundRgbColor = options.ItemBackgroundRgbColor,
        } : null;
        var prefix = !options.FallbackStyle || string.IsNullOrEmpty(options.Prefix) ? null : new ConsoleText(options.Prefix, style);
        var i = 1;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (prefix is null)
            {
                console.Append(ConsoleColor.Blue, i);
                console.Append(ConsoleColor.Blue, i < 10 ? ".  " : ". ");
            }
            else if (string.IsNullOrWhiteSpace(item.Title))
            {
                continue;
            }
            else
            {
                console.Append(prefix);
            }

            console.WriteLine(item.Title, style);
            i++;
        }

        console.Write(new ConsoleTextStyle
        {
            ForegroundConsoleColor = options.QuestionForegroundConsoleColor ?? options.ForegroundColor,
            ForegroundRgbColor = options.QuestionForegroundRgbColor,
            BackgroundConsoleColor = options.QuestionBackgroundConsoleColor ?? options.BackgroundColor,
            BackgroundRgbColor = options.QuestionBackgroundRgbColor,
        }, options.QuestionWhenNotSupported ?? options.ManualQuestion ?? options.Question);
        return SelectInternal(console, console.ReadLine(), data, true);
    }

    private static SelectionResult<T> SelectInternal<T>(StyleConsole console, string s, SelectionData<T> data, bool enableIndex = false)
    {
        if (string.IsNullOrEmpty(s))
            return new SelectionResult<T>(s, SelectionResultTypes.Canceled);
        SelectionItem<T> item = null;
        int i;
        if (s.Trim().Length == 1)
        {
            item = data.Get(s[0], out i);
            if (item != null)
            {
                return new SelectionResult<T>(s, i, item.Data, item.Title);
            }
        }

        i = -1;
        var list = data.ToList();
        foreach (var ele in list)
        {
            i++;
            if (ele is null || !ele.Equals(s)) continue;
            item = ele;
            break;
        }

        if (item is null && enableIndex && int.TryParse(s, out i) && i > 0 && i <= list.Count)
        {
            try
            {
                item = list[i - 1];
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        return item is null
            ? new SelectionResult<T>(s, SelectionResultTypes.Typed)
            : new SelectionResult<T>(s, i, item.Data, item.Title, SelectionResultTypes.Typed);

    }

    private static void RenderSentence(StyleConsole console, ConsoleTextStyle style, string value, int start, int length, bool keepTab, int maxWidth)
    {
        if (start + length > maxWidth) length = maxWidth - start;
        var left = TryGetCursorLeft(console);
        var diff = (left ?? start) - start;
        if (diff > 0)
            console.Backspace(diff);
        else if (diff < 0)
            console.Write(' ', -diff);

        value ??= string.Empty;
        if (keepTab) value = value.Replace("\r\n", " \t").Replace("\r", " \t").Replace("\n", " \t").Replace("\v", " \t");
        else value = value.Replace("\r\n", "  ").Replace("\t", "  ").Replace("\r", "  ").Replace("\n", "  ").Replace("\v", "  ");
        var sb = new StringBuilder();
        var i = 0;
        foreach (var c in value)
        {
            i += GetLetterWidth(c);
            if (i > length) break;
            sb.Append(c);
        }

        console.Write(style, sb);
        var left2 = TryGetCursorLeft(console);
        if (!left2.HasValue)
        {
            return;
        }

        diff = left2.Value - start - length;
        if (diff > 0)
            console.Backspace(diff);
        else if (diff < 0)
            console.Write(' ', -diff);
    }
}
