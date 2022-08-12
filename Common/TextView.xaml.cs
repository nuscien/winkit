using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Trivial.Data;
using Trivial.IO;
using Trivial.Reflection;
using Trivial.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<TextView>;

/// <summary>
/// The text view.
/// </summary>
public sealed partial class TextView : UserControl
{
    /// <summary>
    /// The line model.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// Initializes a new instance of the TextView.Line class.
        /// </summary>
        public Line()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TextView.Line class.
        /// </summary>
        /// <param name="text">The text</param>
        public Line(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of the TextView.Line class.
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="background">The background.</param>
        public Line(string text, Brush background)
            : this(text)
        {
            Background = background;
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the background
        /// </summary>
        public Brush Background { get; }
    }

    /// <summary>
    /// The event arguments of line event.
    /// </summary>
    public class LineEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the LineEventArgs class.
        /// </summary>
        public LineEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the LineEventArgs class.
        /// </summary>
        /// <param name="line">The line number.</param>
        /// <param name="text">The text.</param>
        /// <param name="background">The background.</param>
        public LineEventArgs(int line, string text, Brush background = null)
        {
            LineNumber = line;
            Text = text;
            Background = background;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the background
        /// </summary>
        public Brush Background { get; }
    }

    /// <summary>
    /// The dependency property of item container style.
    /// </summary>
    public static readonly DependencyProperty ItemContainerStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(ItemContainerStyle));

    /// <summary>
    /// The dependency property of text style.
    /// </summary>
    public static readonly DependencyProperty TextStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(TextStyle));

    /// <summary>
    /// The dependency property of line number style.
    /// </summary>
    public static readonly DependencyProperty LineNumberStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(LineNumberStyle));

    /// <summary>
    /// The dependency property of line number width.
    /// </summary>
    public static readonly DependencyProperty LineNumberWidthProperty = DependencyObjectProxy.RegisterProperty(nameof(LineNumberWidth), new GridLength(50));

    /// <summary>
    /// The dependency property of text selection state.
    /// </summary>
    public static readonly DependencyProperty IsTextSelectionEnabledProperty = DependencyObjectProxy.RegisterProperty(nameof(IsTextSelectionEnabled), true);

    /// <summary>
    /// The dependency property of selection mode.
    /// </summary>
    public static readonly DependencyProperty SelectionModeProperty = DependencyObjectProxy.RegisterProperty(nameof(SelectionMode), ListViewSelectionMode.None);

    /// <summary>
    /// The text.
    /// </summary>
    private readonly ObservableCollection<TextViewModel> collection = new();

    /// <summary>
    /// Initializes a new instance of the TextView class.
    /// </summary>
    public TextView()
    {
        InitializeComponent();
        TextElement.ItemsSource = collection;
    }

    /// <summary>
    /// Adds or removes the click event on item.
    /// </summary>
    public event EventHandler<LineEventArgs> ItemClick;

    /// <summary>
    /// Adds or removes the change event on item.
    /// </summary>
    public event SelectionChangedEventHandler SelectionChanged;

    /// <summary>
    /// Gets the count of line.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets or sets the item container style.
    /// </summary>
    public Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the text style.
    /// </summary>
    public Style TextStyle
    {
        get => (Style)GetValue(TextStyleProperty);
        set => SetValue(TextStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the line number style.
    /// </summary>
    public Style LineNumberStyle
    {
        get => (Style)GetValue(LineNumberStyleProperty);
        set => SetValue(LineNumberStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of line number.
    /// </summary>
    public GridLength LineNumberWidth
    {
        get => (GridLength)GetValue(LineNumberWidthProperty);
        set => SetValue(LineNumberWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether text selection is enabled in each line.
    /// </summary>
    public bool IsTextSelectionEnabled
    {
        get => (bool)GetValue(IsTextSelectionEnabledProperty);
        set => SetValue(IsTextSelectionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection behavior for the element.
    /// </summary>
    public ListViewSelectionMode SelectionMode
    {
        get => (ListViewSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets the line number selected.
    /// </summary>
    public int SelectedLineNumber
    {
        get => TextElement.SelectedItem is TextViewModel m ? m.LineNumber : -1;
        set => TextElement.SelectedItem = collection.FirstOrDefault(ele => ele?.LineNumber == value);
    }

    /// <summary>
    /// Gets the line number selected.
    /// </summary>
    public string SelectedLine => TextElement.SelectedItem is TextViewModel m ? m.Text : null;

    /// <summary>
    /// Gets the line number selected.
    /// </summary>
    public IList<int> SelectedLineNumbers => TextElement.SelectedItems?.OfType<TextViewModel>()?.Select(ele => ele.LineNumber)?.ToList();

    /// <summary>
    /// Gets the line number selected.
    /// </summary>
    public IList<string> SelectedLines => TextElement.SelectedItems?.OfType<TextViewModel>()?.Select(ele => ele.Text)?.ToList();

    /// <summary>
    /// Gets a collection of item index range that describe the currently selected items in the list.
    /// </summary>
    public IReadOnlyList<ItemIndexRange> SelectedRanges => TextElement.SelectedRanges;

    /// <summary>
    /// Gets or sets the content of header.
    /// </summary>
    public object Header
    {
        get => TextElement.Header;
        set => TextElement.Header = value;
    }

    /// <summary>
    /// Gets or sets the content of footer.
    /// </summary>
    public object Footer
    {
        get => TextElement.Footer;
        set => TextElement.Footer = value;
    }

    /// <summary>
    /// Appends.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public void Append(string text)
        => Append(new CharsReader(text).ReadLines());

    /// <summary>
    /// Appends.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public void Append(CharsReader text)
        => Append(text?.ReadLines());

    /// <summary>
    /// Gets the text of the specific line.
    /// </summary>
    /// <param name="lineNumber">The line number.</param>
    /// <returns>The text in the line.</returns>
    public string GetLine(int lineNumber)
        => collection.FirstOrDefault(ele => ele?.LineNumber == lineNumber)?.Text;

    /// <summary>
    /// Gets line number by searching a keyword.
    /// </summary>
    /// <param name="q">The keyword to search.</param>
    /// <param name="exactly">true if equaling; otherwise, false.</param>
    /// <param name="start">The start position.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>The line number.</returns>
    public int GetLineNumber(string q, bool exactly = false, int start = 0, StringComparison? comparison = null)
    {
        if (string.IsNullOrEmpty(q)) return -1;
        return SearchInternal(q, exactly, start, comparison).FirstOrDefault()?.LineNumber ?? -1;
    }

    /// <summary>
    /// Gets line number by searching a keyword.
    /// </summary>
    /// <param name="q">The keyword to search.</param>
    /// <param name="exactly">true if equaling; otherwise, false.</param>
    /// <param name="afterSelection">true if find next; otherwise, false.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>The line number.</returns>
    public int GetLineNumber(string q, bool exactly, bool afterSelection, StringComparison? comparison = null)
    {
        if (string.IsNullOrEmpty(q)) return -1;
        var i = afterSelection && TextElement.SelectedItem is TextViewModel m ? (m.LineNumber + 1) : 0;
        return SearchInternal(q, exactly, i, comparison).FirstOrDefault()?.LineNumber ?? -1;
    }

    /// <summary>
    /// Gets line number by searching a keyword.
    /// </summary>
    /// <param name="q">The keyword to search.</param>
    /// <param name="exactly">true if equaling; otherwise, false.</param>
    /// <param name="start">The start position.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>The line number.</returns>
    public int GetLastLineNumber(string q, bool exactly = false, int start = 0, StringComparison? comparison = null)
    {
        if (string.IsNullOrEmpty(q)) return -1;
        return SearchLastInternal(q, exactly, start, comparison).FirstOrDefault()?.LineNumber ?? -1;
    }

    /// <summary>
    /// Gets line number by searching a keyword.
    /// </summary>
    /// <param name="q">The keyword to search.</param>
    /// <param name="exactly">true if equaling; otherwise, false.</param>
    /// <param name="beforeSelection">true if find next; otherwise, false.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>The line number.</returns>
    public int GetLastLineNumber(string q, bool exactly, bool beforeSelection, StringComparison? comparison = null)
    {
        if (string.IsNullOrEmpty(q)) return -1;
        var i = beforeSelection && TextElement.SelectedItem is TextViewModel m ? (m.LineNumber - 1) : -1;
        return SearchLastInternal(q, exactly, i, comparison).FirstOrDefault()?.LineNumber ?? -1;
    }

    /// <summary>
    /// Gets all line numbers by searching a keyword.
    /// </summary>
    /// <param name="q">The keyword to search.</param>
    /// <param name="exactly">true if equaling; otherwise, false.</param>
    /// <param name="start">The start position.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>The collection of line number.</returns>
    public IEnumerable<int> GetLineNumbers(string q, bool exactly = false, int start = 0, StringComparison? comparison = null)
    {
        if (string.IsNullOrEmpty(q)) return new List<int>();
        return SearchInternal(q, exactly, start, comparison).Select(ele => ele.LineNumber).ToList();
    }

    /// <summary>
    /// Scrolls the list to bring the item of the specific line number into view with the specified alignment.
    /// </summary>
    /// <param name="lineNumber">The line number.</param>
    /// <param name="alignment">The alignment to scroll into view.</param>
    public void ScrollIntoView(int lineNumber, ScrollIntoViewAlignment alignment = ScrollIntoViewAlignment.Default)
    {
        var line = collection.FirstOrDefault(ele => ele?.LineNumber == lineNumber);
        if (line != null) TextElement.ScrollIntoView(line, alignment);
    }

    /// <summary>
    /// Selects a block of items described by the item index range.
    /// </summary>
    /// <param name="itemIndexRange">Information about the range of items, including the index of the first and last items in the range, and the number of items.</param>
    public void SelectRange(ItemIndexRange itemIndexRange)
        => TextElement.SelectRange(itemIndexRange);

    /// <summary>
    /// Selects a block of items described by the item index range.
    /// </summary>
    /// <param name="firstIndex">The index of the first item in the instance of the range to select.</param>
    /// <param name="length">The number of items in the instance of the range to select.</param>
    public void SelectRange(int firstIndex, uint length)
        => TextElement.SelectRange(new ItemIndexRange(firstIndex, length));

    /// <summary>
    /// Selects all the items in a view.
    /// </summary>
    public void SelectAll()
        => TextElement.SelectAll();

    /// <summary>
    /// Appends.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public void Append(IEnumerable<string> text)
    {
        if (text == null) return;
        foreach (var line in text)
        {
            Count++;
            var item = new TextViewModel
            {
                Text = line,
                LineNumber = Count
            };
            collection.Add(item);
        }
    }

    /// <summary>
    /// Appends.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public void Append(IEnumerable<Line> text)
    {
        if (text == null) return;
        foreach (var line in text)
        {
            Count++;
            var s = line?.Text;
            var item = new TextViewModel
            {
                Text = s,
                LineNumber = Count
            };
            var background = line?.Background;
            if (background != null) item.Background = background;
            collection.Add(item);
        }
    }

    ///// <summary>
    ///// Appends.
    ///// </summary>
    ///// <param name="text">The text to append.</param>
    ///// <param name="style">The optional style for JSON.</param>
    //public void Append(JsonObjectNode text, JsonTextStyle style = null)
    //{
    //    if (text == null) return;
    //    var inlines = VisualUtilities.CreateTextInlines(text, style);
    //    var indexWidth = new GridLength(double.IsNaN(IndexWidth) || IndexWidth < 0 ? 0 : IndexWidth);
    //    TextViewModel item = null;
    //    var col = new List<TextViewModel>();
    //    foreach (var line in inlines)
    //    {
    //        if (item == null)
    //        {
    //            Count++;
    //            item = new TextViewModel
    //            {
    //                TextStyle = TextStyle,
    //                LineNumber = Count,
    //                LineNumberWidth = indexWidth,
    //                LineNumberStyle = LineNumberStyle
    //            };
    //            col.Add(item);
    //        }

    //        if (line is LineBreak)
    //        {
    //            item = null;
    //            continue;
    //        }

    //        item.Inlines.Add(line);
    //    }

    //    foreach (var ele in col)
    //    {
    //        collection.Add(ele);
    //    }
    //}

    /// <summary>
    /// Sets text.
    /// </summary>
    /// <param name="text">The text to set.</param>
    public void SetData(string text)
        => SetData(new CharsReader(text).ReadLines());

    /// <summary>
    /// Sets text.
    /// </summary>
    /// <param name="text">The text to set.</param>
    public void SetData(CharsReader text)
        => SetData(text?.ReadLines());

    /// <summary>
    /// Sets text.
    /// </summary>
    /// <param name="text">The text to set.</param>
    public void SetData(IEnumerable<string> text)
    {
        collection.Clear();
        Count = 0;
        if (text == null) return;
        Append(text);
    }

    private IEnumerable<TextViewModel> SearchInternal(string q, bool exactly = false, int start = 0, StringComparison? comparison = null)
    {
        if (string.IsNullOrEmpty(q)) return new List<TextViewModel>();
        var col = collection.Where(ele => ele != null && ele.LineNumber >= start);
        if (exactly)
            return comparison.HasValue ? col.Where(ele => q.Equals(ele.Text, comparison.Value)) : col.Where(ele => q.Equals(ele.Text));
        return comparison.HasValue ? col.Where(ele => !string.IsNullOrEmpty(ele.Text) && ele.Text.Contains(q, comparison.Value)) : col.Where(ele => !string.IsNullOrEmpty(ele.Text) && ele.Text.Contains(q));
    }

    private IEnumerable<TextViewModel> SearchLastInternal(string q, bool exactly = false, int start = 0, StringComparison? comparison = null)
    {
        if (string.IsNullOrEmpty(q)) return new List<TextViewModel>();
        var col = start == -1 ? collection.Where(ele => ele != null) : collection.Where(ele => ele != null && ele.LineNumber <= start);
        col = col.OrderByDescending(ele => ele.LineNumber);
        if (exactly)
            return comparison.HasValue ? col.Where(ele => q.Equals(ele.Text, comparison.Value)) : col.Where(ele => q.Equals(ele.Text));
        return comparison.HasValue ? col.Where(ele => !string.IsNullOrEmpty(ele.Text) && ele.Text.Contains(q, comparison.Value)) : col.Where(ele => !string.IsNullOrEmpty(ele.Text) && ele.Text.Contains(q));
    }

    private void TextElement_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not TextViewModel model) return;
        ItemClick?.Invoke(this, new LineEventArgs(model.LineNumber, model.Text, model.Background));
    }

    private void TextElement_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
    }
}

internal class TextViewModel : ObservableProperties
{
    public int LineNumber
    {
        get => GetCurrentProperty<int>();
        internal set => SetCurrentProperty(value);
    }

    public string Text
    {
        get => GetCurrentProperty<string>();
        internal set => SetCurrentProperty(value);
    }

    public Brush Background
    {
        get => GetCurrentProperty<Brush>();
        internal set => SetCurrentProperty(value);
    }
}
