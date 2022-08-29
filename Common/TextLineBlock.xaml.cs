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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Trivial.Data;
using Trivial.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<TextLineBlock>;

/// <summary>
/// The text block for a line.
/// </summary>
public sealed partial class TextLineBlock : UserControl
{
    /// <summary>
    /// The dependency property of index column width.
    /// </summary>
    public static readonly DependencyProperty IndexWidthProperty = DependencyObjectProxy.RegisterProperty(nameof(IndexWidth), new GridLength(60));

    /// <summary>
    /// The dependency property of prefix column width.
    /// </summary>
    public static readonly DependencyProperty PrefixWidthProperty = DependencyObjectProxy.RegisterProperty(nameof(PrefixWidth), new GridLength(20));

    /// <summary>
    /// The dependency property of line height.
    /// </summary>
    public static readonly DependencyProperty LineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(LineHeight), 18d);

    /// <summary>
    /// The dependency property of index.
    /// </summary>
    public static readonly DependencyProperty IndexProperty = DependencyObjectProxy.RegisterProperty(nameof(Index), null, null, obj =>
    {
        if (obj is null) return null;
        if (obj is int i) return i.ToString("g");
        if (obj is long l) return l.ToString("g");
        return obj.ToString();
    });

    /// <summary>
    /// The dependency property of prefix.
    /// </summary>
    public static readonly DependencyProperty PrefixProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Prefix));

    /// <summary>
    /// The dependency property of text.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Text));

    /// <summary>
    /// The dependency property of text highlighters.
    /// </summary>
    public static readonly DependencyProperty TextHighlightersProperty = DependencyObjectProxy.RegisterProperty<IEnumerable<TextHighlighter>>(nameof(TextHighlighters), OnTextHighlightersChanged);

    /// <summary>
    /// Initializes a new instance of the TextLineBlock class.
    /// </summary>
    public TextLineBlock()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the width of index column.
    /// </summary>
    public GridLength IndexWidth
    {
        get => (GridLength)GetValue(IndexWidthProperty);
        set => SetValue(IndexWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of prefix column.
    /// </summary>
    public GridLength PrefixWidth
    {
        get => (GridLength)GetValue(PrefixWidthProperty);
        set => SetValue(PrefixWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the line height.
    /// </summary>
    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public string Index
    {
        get => (string)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the prefix.
    /// </summary>
    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets the collection of inline text elements within a text.
    /// </summary>
    public InlineCollection Inlines => TextElement.Inlines;

    /// <summary>
    /// Gets or sets the collection of text highlighter.
    /// </summary>
    public IEnumerable<TextHighlighter> TextHighlighters
    {
        get => (IEnumerable<TextHighlighter>)GetValue(TextHighlightersProperty);
        set => SetValue(TextHighlightersProperty, value);
    }

    private static void OnTextHighlightersChanged(TextLineBlock c, ChangeEventArgs<IEnumerable<TextHighlighter>> e, DependencyProperty d)
    {
        if (c == null || e?.NewValue == null) return;
        c.TextElement.TextHighlighters.Clear();
        foreach (var item in e.NewValue)
        {
            c.TextElement.TextHighlighters.Add(item);
        }
    }
}
