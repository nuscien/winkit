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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<BlockHeader>;

public sealed partial class BlockHeader : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Title));
    public static readonly DependencyProperty TitleFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleFontSize), 16d);
    public static readonly DependencyProperty TitleFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(TitleFontWeight));
    public static readonly DependencyProperty TitleFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(TitleFontStyle));
    public static readonly DependencyProperty TitleLineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleLineHeight), 0d);
    public static readonly DependencyProperty TitleForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TitleForeground));
    public static readonly DependencyProperty IconUriProperty = DependencyObjectProxy.RegisterProperty<Uri>(nameof(IconUri));
    public static readonly DependencyProperty IconWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconWidth));
    public static readonly DependencyProperty IconHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconHeight));
    public static readonly DependencyProperty IconMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(IconMargin));

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
}
