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
    public static readonly DependencyProperty TextCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(TextCornerRadius));

    /// <summary>
    /// The dependency property of line number.
    /// </summary>
    public static readonly DependencyProperty LineNumberProperty = DependencyObjectProxy.RegisterProperty(nameof(LineNumber), null, null, obj =>
    {
        if (obj is null) return null;
        if (obj is int i) return i.ToString("g");
        if (obj is long l) return l.ToString("g");
        return obj.ToString();
    });

    /// <summary>
    /// The dependency property of text selection state.
    /// </summary>
    public static readonly DependencyProperty IsTextSelectionEnabledProperty = DependencyObjectProxy.RegisterProperty(nameof(IsTextSelectionEnabled), true);

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
    /// The dependency property of text background.
    /// </summary>
    public static readonly DependencyProperty TextBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TextBackground));

    /// <summary>
    /// The dependency property of text style.
    /// </summary>
    public static readonly DependencyProperty TextStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(TextStyle));

    /// <summary>
    /// The dependency property of prefix text style.
    /// </summary>
    public static readonly DependencyProperty PrefixStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(PrefixStyle));

    /// <summary>
    /// The dependency property of line number style.
    /// </summary>
    public static readonly DependencyProperty LineNumberStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(LineNumberStyle));

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
    /// Gets or sets the corner radius of text background.
    /// </summary>
    public CornerRadius TextCornerRadius
    {
        get => (CornerRadius)GetValue(TextCornerRadiusProperty);
        set => SetValue(TextCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the line number in string.
    /// </summary>
    public string LineNumber
    {
        get => (string)GetValue(LineNumberProperty);
        set => SetValue(LineNumberProperty, value);
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
    /// Gets or sets the prefix text.
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
    /// Gets or sets the text background.
    /// </summary>
    public Brush TextBackground
    {
        get => (Brush)GetValue(TextBackgroundProperty);
        set => SetValue(TextBackgroundProperty, value);
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
    /// Gets or sets the prefix text style.
    /// </summary>
    public Style PrefixStyle
    {
        get => (Style)GetValue(PrefixStyleProperty);
        set => SetValue(PrefixStyleProperty, value);
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

    /// <summary>
    /// Sets the model.
    /// </summary>
    /// <param name="model">The model.</param>
    public void SetModel(TextLineBlockModel model)
    {
        if (model == null) model = new();
        Text = model.Text;
        TextBackground = model.Background;
        TextHighlighters = model.TextHighlighters;
        Prefix = model.Prefix;
    }

    /// <summary>
    /// Converts to the text line block model.
    /// </summary>
    /// <param name="value">The element.</param>
    public static explicit operator TextLineBlockModel(TextLineBlock value)
    {
        if (value == null) return null;
        return new(value.Text, value.TextHighlighters, value.TextBackground, value.Prefix);
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

/// <summary>
/// The line model.
/// </summary>
public class TextLineBlockModel
{
    /// <summary>
    /// Initializes a new instance of the TextLineBlockModel class.
    /// </summary>
    public TextLineBlockModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the TextLineBlockModel class.
    /// </summary>
    /// <param name="text">The text</param>
    public TextLineBlockModel(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the TextLineBlockModel class.
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="background">The background.</param>
    public TextLineBlockModel(string text, Brush background)
        : this(text)
    {
        Background = background;
    }

    /// <summary>
    /// Initializes a new instance of the TextLineBlockModel class.
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="highlighters">The text highlighters.</param>
    /// <param name="background">The background.</param>
    /// <param name="prefix">The prefix.</param>
    public TextLineBlockModel(string text, IEnumerable<TextHighlighter> highlighters, Brush background, string prefix = null)
        : this(text, background)
    {
        Prefix = prefix;
        if (highlighters == null) return;
        foreach (var item in highlighters)
        {
            TextHighlighters.Add(item);
        }
    }

    /// <summary>
    /// Gets the text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the prefix text.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Gets the background
    /// </summary>
    public Brush Background { get; }

    /// <summary>
    /// Gets the text highlighters.
    /// </summary>
    public IList<TextHighlighter> TextHighlighters { get; } = new ObservableCollection<TextHighlighter>();

    /// <summary>
    /// Adds a text highlighter.
    /// </summary>
    /// <param name="obj">The instance of text highlighter to add.</param>
    /// <param name="ranges">The additional text ranges.</param>
    public void AddTextHighlighter(TextHighlighter obj, params TextRange[] ranges)
    {
        if (obj == null) return;
        if (ranges != null)
        {
            foreach (var range in ranges)
            {
                obj.Ranges.Add(range);
            }
        }

        TextHighlighters.Add(obj);
    }
}
