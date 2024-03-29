﻿using Microsoft.UI.Xaml;
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
using DependencyObjectProxy = DependencyObjectProxy<BlockHeader>;

/// <summary>
/// The block header control.
/// </summary>
public sealed partial class BlockHeader : UserControl
{
    /// <summary>
    /// The dependency property of title.
    /// </summary>
    public static readonly DependencyProperty TitleProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Title));

    /// <summary>
    /// The dependency property of title font size.
    /// </summary>
    public static readonly DependencyProperty TitleFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleFontSize), 16d);

    /// <summary>
    /// The dependency property of font weight.
    /// </summary>
    public static readonly DependencyProperty TitleFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(TitleFontWeight));

    /// <summary>
    /// The dependency property of font style.
    /// </summary>
    public static readonly DependencyProperty TitleFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(TitleFontStyle));

    /// <summary>
    /// The dependency property of line height.
    /// </summary>
    public static readonly DependencyProperty TitleLineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleLineHeight), 0d);

    /// <summary>
    /// The dependency property of foreground.
    /// </summary>
    public static readonly DependencyProperty TitleForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TitleForeground));

    /// <summary>
    /// The dependency property of icon URI.
    /// </summary>
    public static readonly DependencyProperty IconUriProperty = DependencyObjectProxy.RegisterProperty<Uri>(nameof(IconUri), OnIconChanged);

    /// <summary>
    /// The dependency property of icon stretch.
    /// </summary>
    public static readonly DependencyProperty IconStretchProperty = DependencyObjectProxy.RegisterProperty<Stretch>(nameof(IconStretch));

    /// <summary>
    /// The dependency property of icon width.
    /// </summary>
    public static readonly DependencyProperty IconWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconWidth));

    /// <summary>
    /// The dependency property of icon height.
    /// </summary>
    public static readonly DependencyProperty IconHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconHeight));

    /// <summary>
    /// The dependency property of icon margin.
    /// </summary>
    public static readonly DependencyProperty IconMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(IconMargin));

    /// <summary>
    /// The dependency property of icon corner radius.
    /// </summary>
    public static readonly DependencyProperty IconCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(IconCornerRadius));

    /// <summary>
    /// The dependency property of left margin.
    /// </summary>
    public static readonly DependencyProperty LeftMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(LeftMargin));

    /// <summary>
    /// The dependency property of right margin.
    /// </summary>
    public static readonly DependencyProperty RightMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(RightMargin));

    /// <summary>
    /// Initializes a new instance of the BlockHeader class.
    /// </summary>
    public BlockHeader()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size of title.
    /// </summary>
    public double TitleFontSize
    {
        get => (double)GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the font weight of title.
    /// </summary>
    public FontWeight TitleFontWeight
    {
        get => (FontWeight)GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the font style of title.
    /// </summary>
    public FontStyle TitleFontStyle
    {
        get => (FontStyle)GetValue(TitleFontStyleProperty);
        set => SetValue(TitleFontStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground of title.
    /// </summary>
    public Brush TitleForeground
    {
        get => (Brush)GetValue(TitleForegroundProperty);
        set => SetValue(TitleForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the line height of title.
    /// </summary>
    public double TitleLineHeight
    {
        get => (double)GetValue(TitleLineHeightProperty);
        set => SetValue(TitleLineHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon URI.
    /// </summary>
    public Uri IconUri
    {
        get => (Uri)GetValue(IconUriProperty);
        set => SetValue(IconUriProperty, value);
    }

    /// <summary>
    /// Gets or sets the stretch of icon.
    /// </summary>
    public Stretch IconStretch
    {
        get => (Stretch)GetValue(IconStretchProperty);
        set => SetValue(IconStretchProperty, value);
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
    /// Gets or sets the margin of icon.
    /// </summary>
    public Thickness IconMargin
    {
        get => (Thickness)GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of icon.
    /// </summary>
    public CornerRadius IconCornerRadius
    {
        get => (CornerRadius)GetValue(IconCornerRadiusProperty);
        set => SetValue(IconCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin of left zone.
    /// </summary>
    public Thickness LeftMargin
    {
        get => (Thickness)GetValue(LeftMarginProperty);
        set => SetValue(LeftMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin of right zone.
    /// </summary>
    public Thickness RightMargin
    {
        get => (Thickness)GetValue(RightMarginProperty);
        set => SetValue(RightMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of customized zone before content.
    /// </summary>
    public UIElement BeforeContent
    {
        get => BeforePanel.Child;
        set => BeforePanel.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of customized zone after content.
    /// </summary>
    public UIElement AfterContent
    {
        get => AfterPanel.Child;
        set => AfterPanel.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of content background.
    /// </summary>
    public UIElement BackgroundContent
    {
        get => BackgroundPanel.Child;
        set => BackgroundPanel.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of content cover.
    /// </summary>
    public UIElement CoverContent
    {
        get => CoverPanel.Child;
        set => CoverPanel.Child = value;
    }

    /// <summary>
    /// Gets the children of right panel.
    /// </summary>
    public UIElementCollection RightChildren => RightPanel.Children;

    private static void OnIconChanged(BlockHeader c, ChangeEventArgs<Uri> e, DependencyProperty p)
    {
        c.IconPanel.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;
    }
}
