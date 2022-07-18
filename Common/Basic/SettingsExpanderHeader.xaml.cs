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
using Trivial.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<SettingsExpanderHeader>;

/// <summary>
/// The common expander header for settings section.
/// </summary>
public sealed partial class SettingsExpanderHeader : UserControl
{
    /// <summary>
    /// The dependency property of icon glyph property.
    /// </summary>
    public static readonly DependencyProperty IconGlyphProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(IconGlyph), OnTextChanged);

    /// <summary>
    /// The dependency property of icon width.
    /// </summary>
    public static readonly DependencyProperty IconWidthProperty = DependencyObjectProxy.RegisterProperty(nameof(IconWidth), 24d);

    /// <summary>
    /// The dependency property of icon height.
    /// </summary>
    public static readonly DependencyProperty IconHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(IconHeight), 24d);

    /// <summary>
    /// The dependency property of icon spacing.
    /// </summary>
    public static readonly DependencyProperty IconSpacingProperty = DependencyObjectProxy.RegisterProperty(nameof(IconSpacing), 16d);

    /// <summary>
    /// The dependency property of title property.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Text), OnTextChanged);

    /// <summary>
    /// The dependency property of title line height.
    /// </summary>
    public static readonly DependencyProperty LineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(LineHeight), 0d);

    /// <summary>
    /// The dependency property of description.
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Description), OnTextChanged);

    /// <summary>
    /// The dependency property of description font size.
    /// </summary>
    public static readonly DependencyProperty DescriptionFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(DescriptionFontSize), 12d);

    /// <summary>
    /// The dependency property of description font weight.
    /// </summary>
    public static readonly DependencyProperty DescriptionFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(DescriptionFontWeight));

    /// <summary>
    /// The dependency property of description font style.
    /// </summary>
    public static readonly DependencyProperty DescriptionFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(DescriptionFontStyle));

    /// <summary>
    /// The dependency property of description line height.
    /// </summary>
    public static readonly DependencyProperty DescriptionLineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(DescriptionLineHeight), 0d);

    /// <summary>
    /// The dependency property of description foreground.
    /// </summary>
    public static readonly DependencyProperty DescriptionForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DescriptionForeground));

    /// <summary>
    /// Initializes a new instance of the SettingsExpanderHeader class.
    /// </summary>
    public SettingsExpanderHeader()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the icon glyph.
    /// </summary>
    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of icon.
    /// </summary>
    public double IconWidth
    {
        get => (double)GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of icon.
    /// </summary>
    public double IconHeight
    {
        get => (double)GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between icon and title.
    /// </summary>
    public double IconSpacing
    {
        get => (double)GetValue(IconSpacingProperty);
        set => SetValue(IconSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the line height of title.
    /// </summary>
    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
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
    /// Gets or sets the font size of description.
    /// </summary>
    public double DescriptionFontSize
    {
        get => (double)GetValue(DescriptionFontSizeProperty);
        set => SetValue(DescriptionFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the font weight of description.
    /// </summary>
    public FontWeight DescriptionFontWeight
    {
        get => (FontWeight)GetValue(DescriptionFontWeightProperty);
        set => SetValue(DescriptionFontWeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the font style of description.
    /// </summary>
    public FontStyle DescriptionFontStyle
    {
        get => (FontStyle)GetValue(DescriptionFontStyleProperty);
        set => SetValue(DescriptionFontStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the line height of description.
    /// </summary>
    public double DescriptionLineHeight
    {
        get => (double)GetValue(DescriptionLineHeightProperty);
        set => SetValue(DescriptionLineHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground of description.
    /// </summary>
    public Brush DescriptionForeground
    {
        get => (Brush)GetValue(DescriptionForegroundProperty);
        set => SetValue(DescriptionForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the child element in right panel.
    /// </summary>
    public UIElement RightContent
    {
        get => RightPanel.Child;
        set => RightPanel.Child = value;
    }

    private static void OnTextChanged(SettingsExpanderHeader c, ChangeEventArgs<string> e, DependencyProperty p)
    {
        c.TitleText.Visibility = string.IsNullOrWhiteSpace(c.Text) ? Visibility.Collapsed : Visibility.Visible;
        c.DescriptionText.Visibility = string.IsNullOrWhiteSpace(c.Description) ? Visibility.Collapsed : Visibility.Visible;
        c.IconElement.Visibility = string.IsNullOrWhiteSpace(c.IconGlyph) ? Visibility.Collapsed : Visibility.Visible;
    }
}
