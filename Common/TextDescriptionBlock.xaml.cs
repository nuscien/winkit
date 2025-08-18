using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<TextDescriptionBlock>;

/// <summary>
/// The text block with title and description.
/// </summary>
public sealed partial class TextDescriptionBlock : UserControl
{
    /// <summary>
    /// The dependency property of icon width property.
    /// </summary>
    public static readonly DependencyProperty IconWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconWidth));

    /// <summary>
    /// The dependency property of icon height property.
    /// </summary>
    public static readonly DependencyProperty IconHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconHeight));

    /// <summary>
    /// The dependency property of icon spacing property.
    /// </summary>
    public static readonly DependencyProperty IconSpacingProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconSpacing), (c, e, p)=>
    {
        c.IconElement.Margin = new(0d, 0d, double.IsNaN(e.NewValue) || e.NewValue < 0d ? 0d : e.NewValue, 0d);
    }, 0d);

    /// <summary>
    /// The dependency property of text property.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Text));

    /// <summary>
    /// The dependency property of text trimming property.
    /// </summary>
    public static readonly DependencyProperty TextTrimmingProperty = DependencyObjectProxy.RegisterProperty<TextTrimming>(nameof(TextTrimming));

    /// <summary>
    /// The dependency property of title maximum width property.
    /// </summary>
    public static readonly DependencyProperty TextMaxWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TextMaxWidth), null, double.PositiveInfinity);

    /// <summary>
    /// The dependency property of title minimum width property.
    /// </summary>
    public static readonly DependencyProperty TextMinWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TextMinWidth), null, 0d);

    /// <summary>
    /// The dependency property of description property.
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Description));

    /// <summary>
    /// The dependency property of description spacing property.
    /// </summary>
    public static readonly DependencyProperty DescriptionSpacingProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionSpacing), (c, e, p) =>
    {
        var v = double.IsNaN(e.NewValue) ? 0d : e.NewValue;
        c.DescriptionElement.Margin = new(v, 0d, v, 0d);
    }, 0d);

    /// <summary>
    /// The dependency property of description maximum width property.
    /// </summary>
    public static readonly DependencyProperty DescriptionMaxWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionMaxWidth), null, double.PositiveInfinity);

    /// <summary>
    /// The dependency property of description minimum width property.
    /// </summary>
    public static readonly DependencyProperty DescriptionMinWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionMinWidth), null, 0d);

    /// <summary>
    /// The dependency property of description foreground.
    /// </summary>
    public static readonly DependencyProperty DescriptionForegroundProperty = DependencyObjectProxy.RegisterBrushProperty(nameof(DescriptionForeground));

    /// <summary>
    /// Initializes a new instance of the TextDescriptionBlock class.
    /// </summary>
    public TextDescriptionBlock()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the TextDescriptionBlock class.
    /// </summary>
    /// <param name="icon">The icon.</param>
    /// <param name="title">The title.</param>
    /// <param name="descripton">The description.</param>
    public TextDescriptionBlock(IconSource icon, string title, string descripton = null)
    {
        InitializeComponent();
        Text = title;
        Description = descripton;
        Icon = icon;
    }

    /// <summary>
    /// Initializes a new instance of the TextDescriptionBlock class.
    /// </summary>
    /// <param name="symbol">The symbol used to show in icon.</param>
    /// <param name="title">The title.</param>
    /// <param name="descripton">The description.</param>
    public TextDescriptionBlock(Symbol symbol, string title, string descripton = null)
        : this(new SymbolIconSource
        {
            Symbol = symbol,
        }, title, descripton)
    {
    }

    /// <summary>
    /// Initializes a new instance of the TextDescriptionBlock class.
    /// </summary>
    /// <param name="symbol">The symbol used to show in icon.</param>
    /// <param name="foregroud">The forground brush of icon.</param>
    /// <param name="title">The title.</param>
    /// <param name="descripton">The description.</param>
    public TextDescriptionBlock(Symbol symbol, Brush foregroud, string title, string descripton = null)
        : this(new SymbolIconSource
        {
            Symbol = symbol,
            Foreground = foregroud,
        }, title, descripton)
    {
    }

    /// <summary>
    /// Gets or sets the image source of the icon.
    /// </summary>
    public IconSource Icon
    {
        get => IconElement.IconSource;
        set => IconElement.IconSource = value;
    }

    /// <summary>
    /// Gets or sets the width of the icon.
    /// </summary>
    public double IconWidth
    {
        get => (double)GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the icon.
    /// </summary>
    public double IconHeight
    {
        get => (double)GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between icon and text.
    /// </summary>
    public double IconSpacing
    {
        get => (double)GetValue(IconSpacingProperty);
        set => SetValue(IconSpacingProperty, value);
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
    /// Gets or sets the text trimming.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of text.
    /// </summary>
    public double TextMaxWidth
    {
        get => (double)GetValue(TextMaxWidthProperty);
        set => SetValue(TextMaxWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of text.
    /// </summary>
    public double TextMinWidth
    {
        get => (double)GetValue(TextMinWidthProperty);
        set => SetValue(TextMinWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between text and description.
    /// </summary>
    public double DescriptionSpacing
    {
        get => (double)GetValue(DescriptionSpacingProperty);
        set => SetValue(DescriptionSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of description.
    /// </summary>
    public double DescriptionMaxWidth
    {
        get => (double)GetValue(DescriptionMaxWidthProperty);
        set => SetValue(DescriptionMaxWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of description.
    /// </summary>
    public double DescriptionMinWidth
    {
        get => (double)GetValue(DescriptionMinWidthProperty);
        set => SetValue(DescriptionMinWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush of description.
    /// </summary>
    public Brush DescriptionForeground
    {
        get => (Brush)GetValue(DescriptionForegroundProperty);
        set => SetValue(DescriptionForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the appended content.
    /// </summary>
    public UIElement Appended
    {
        get => RestElement.Child;
        set => RestElement.Child = value;
    }

    /// <summary>
    /// Gets or sets the header content.
    /// </summary>
    public UIElement Header
    {
        get => LeftElement.Child;
        set => LeftElement.Child = value;
    }

    /// <summary>
    /// Gets or sets the footer content.
    /// </summary>
    public UIElement Footer
    {
        get => RightElement.Child;
        set => RightElement.Child = value;
    }

    /// <summary>
    /// Gets or sets the background content.
    /// </summary>
    public UIElement BackgroundContent
    {
        get => BackgroundElement.Child;
        set => BackgroundElement.Child = value;
    }

    /// <inheritdoc />
    public override string ToString()
        => Text ?? string.Empty;
}
