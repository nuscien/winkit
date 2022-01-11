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
using DependencyObjectProxy = DependencyObjectProxy<TileCollection>;

public sealed partial class TileCollection : UserControl
{
    public static readonly DependencyProperty ItemStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(ItemStyle), OnItemStyleChanged);
    public static readonly DependencyProperty ScrollViewStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(ScrollViewStyle), (c, e, p) => c.ScrollViewStyle = e.NewValue);
    public static readonly DependencyProperty HeaderHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(HeaderHeight), (c, e, p) => c.HeaderHeight = e.NewValue);
    public static readonly DependencyProperty HeaderFontSizeProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(HeaderFontSize), (c, e, p) => c.HeaderFontSize = e.NewValue, 12);
    public static readonly DependencyProperty HeaderFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(HeaderFontWeight), (c, e, p) => c.HeaderFontWeight = e.NewValue);
    public static readonly DependencyProperty HeaderFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(HeaderFontStyle), (c, e, p) => c.HeaderFontStyle = e.NewValue);
    public static readonly DependencyProperty HeaderForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HeaderForeground), (c, e, p) => c.HeaderForeground = e.NewValue);
    public static readonly DependencyProperty HeaderBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HeaderBackground), (c, e, p) => c.HeaderBackground = e.NewValue);
    public static readonly DependencyProperty HeaderBackgroundSizingProperty = DependencyObjectProxy.RegisterProperty<BackgroundSizing>(nameof(HeaderBackgroundSizing), (c, e, p) => c.HeaderBackgroundSizing = e.NewValue);
    public static readonly DependencyProperty HeaderPaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(HeaderPadding), (c, e, p) => c.HeaderPadding = e.NewValue);
    public static readonly DependencyProperty HeaderMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(HeaderMargin), (c, e, p) => c.HeaderMargin = e.NewValue);
    public static readonly DependencyProperty HeaderBorderThicknessProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(HeaderBorderThickness), (c, e, p) => c.HeaderBorderThickness = e.NewValue);
    public static readonly DependencyProperty HeaderBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HeaderBorderBrush), (c, e, p) => c.HeaderBorderBrush = e.NewValue);
    public static readonly DependencyProperty HeaderCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(HeaderCornerRadius), (c, e, p) => c.HeaderCornerRadius = e.NewValue);
    public static readonly DependencyProperty TitleProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Title), (c, e, p) => c.Title = e.NewValue);
    public static readonly DependencyProperty TitleFontSizeProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TitleFontSize), (c, e, p) => c.TitleFontSize = e.NewValue, 18);
    public static readonly DependencyProperty TitleFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(TitleFontWeight), (c, e, p) => c.TitleFontWeight = e.NewValue);
    public static readonly DependencyProperty TitleFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(TitleFontStyle), (c, e, p) => c.TitleFontStyle = e.NewValue);
    public static readonly DependencyProperty TitleLineHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TitleLineHeight), (c, e, p) => c.TitleLineHeight = e.NewValue);
    public static readonly DependencyProperty TitleForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TitleForeground), (c, e, p) => c.TitleForeground = e.NewValue);
    public static readonly DependencyProperty IconUriProperty = DependencyObjectProxy.RegisterProperty<Uri>(nameof(IconUri), (c, e, p) => c.IconUri = e.NewValue);
    public static readonly DependencyProperty IconWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconWidth), (c, e, p) => c.IconWidth = e.NewValue);
    public static readonly DependencyProperty IconHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconHeight), (c, e, p) => c.IconHeight = e.NewValue);
    public static readonly DependencyProperty IconMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(IconMargin), (c, e, p) => c.IconMargin = e.NewValue);

    /// <summary>
    /// Initializes a new instance of the TileCollection class.
    /// </summary>
    public TileCollection()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the children.
    /// </summary>
    public UIElementCollection Children => ListPanel.Children;

    /// <summary>
    /// Gets or sets the style of tile item.
    /// </summary>
    public Style ItemStyle
    {
        get => (Style)GetValue(ItemStyleProperty);
        set => SetValue(ItemStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of internal scroll view.
    /// </summary>
    public Style ScrollViewStyle
    {
        get => ListScrollView.Style;
        set => ListScrollView.Style = value;
    }

    /// <summary>
    /// Gets or sets the font size of header.
    /// </summary>
    public double HeaderFontSize
    {
        get => HeaderElement.FontSize;
        set => HeaderElement.FontSize = value;
    }

    /// <summary>
    /// Gets or sets the font weight of header.
    /// </summary>
    public FontWeight HeaderFontWeight
    {
        get => HeaderElement.FontWeight;
        set => HeaderElement.FontWeight = value;
    }

    /// <summary>
    /// Gets or sets the font style of header.
    /// </summary>
    public FontStyle HeaderFontStyle
    {
        get => HeaderElement.FontStyle;
        set => HeaderElement.FontStyle = value;
    }

    /// <summary>
    /// Gets or sets the height of header.
    /// </summary>
    public double HeaderHeight
    {
        get => HeaderElement.Height;
        set => HeaderElement.Height = value;
    }

    /// <summary>
    /// Gets or sets the foreground of header.
    /// </summary>
    public Brush HeaderForeground
    {
        get => HeaderElement.Foreground;
        set => HeaderElement.Foreground = value;
    }

    /// <summary>
    /// Gets or sets the background of header.
    /// </summary>
    public Brush HeaderBackground
    {
        get => HeaderElement.Background;
        set => HeaderElement.Background = value;
    }

    /// <summary>
    /// Gets or sets the background sizing of header.
    /// </summary>
    public BackgroundSizing HeaderBackgroundSizing
    {
        get => HeaderElement.BackgroundSizing;
        set => HeaderElement.BackgroundSizing = value;
    }

    /// <summary>
    /// Gets or sets the padding of header.
    /// </summary>
    public Thickness HeaderPadding
    {
        get => HeaderElement.Padding;
        set => HeaderElement.Padding = value;
    }

    /// <summary>
    /// Gets or sets the margin of header.
    /// </summary>
    public Thickness HeaderMargin
    {
        get => HeaderElement.Margin;
        set => HeaderElement.Margin = value;
    }

    /// <summary>
    /// Gets or sets the border thickness of header.
    /// </summary>
    public Thickness HeaderBorderThickness
    {
        get => HeaderElement.BorderThickness;
        set => HeaderElement.BorderThickness = value;
    }

    /// <summary>
    /// Gets or sets the border brush of header.
    /// </summary>
    public Brush HeaderBorderBrush
    {
        get => HeaderElement.BorderBrush;
        set => HeaderElement.BorderBrush = value;
    }

    /// <summary>
    /// Gets or sets the corner radius of header.
    /// </summary>
    public CornerRadius HeaderCornerRadius
    {
        get => HeaderElement.CornerRadius;
        set => HeaderElement.CornerRadius = value;
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title
    {
        get => HeaderElement.Title;
        set => HeaderElement.Title = value;
    }

    /// <summary>
    /// Gets or sets the font size of title.
    /// </summary>
    public double TitleFontSize
    {
        get => HeaderElement.TitleFontSize;
        set => HeaderElement.TitleFontSize = value;
    }

    /// <summary>
    /// Gets or sets the font weight of title.
    /// </summary>
    public FontWeight TitleFontWeight
    {
        get => HeaderElement.TitleFontWeight;
        set => HeaderElement.TitleFontWeight = value;
    }

    /// <summary>
    /// Gets or sets the font style of title.
    /// </summary>
    public FontStyle TitleFontStyle
    {
        get => HeaderElement.TitleFontStyle;
        set => HeaderElement.TitleFontStyle = value;
    }

    /// <summary>
    /// Gets or sets the line height of title.
    /// </summary>
    public double TitleLineHeight
    {
        get => HeaderElement.TitleLineHeight;
        set => HeaderElement.TitleLineHeight = value;
    }

    /// <summary>
    /// Gets or sets the foreground of title.
    /// </summary>
    public Brush TitleForeground
    {
        get => HeaderElement.TitleForeground;
        set => HeaderElement.TitleForeground = value;
    }

    /// <summary>
    /// Gets or sets the icon URI.
    /// </summary>
    public Uri IconUri
    {
        get => HeaderElement.IconUri;
        set => HeaderElement.IconUri = value;
    }

    /// <summary>
    /// Gets or sets the width of icon.
    /// </summary>
    public double IconWidth
    {
        get => HeaderElement.IconWidth;
        set => HeaderElement.IconWidth = value;
    }

    /// <summary>
    /// Gets or sets the height of icon.
    /// </summary>
    public double IconHeight
    {
        get => HeaderElement.IconHeight;
        set => HeaderElement.IconHeight = value;
    }

    /// <summary>
    /// Gets or sets the margin of icon.
    /// </summary>
    public Thickness IconMargin
    {
        get => HeaderElement.IconMargin;
        set => HeaderElement.IconMargin = value;
    }

    /// <summary>
    /// Gets or sets the content of top customized zone.
    /// </summary>
    public UIElement TopContent
    {
        get => Top.Child;
        set => Top.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of bottom customized zone.
    /// </summary>
    public UIElement BottomContent
    {
        get => Bottom.Child;
        set => Bottom.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of header cover zone.
    /// </summary>
    public UIElement HeaderCoverContent
    {
        get => HeaderElement.CoverContent;
        set => HeaderElement.CoverContent = value;
    }

    /// <summary>
    /// Gets or sets the content of header background zone.
    /// </summary>
    public UIElement HeaderBackgroundContent
    {
        get => HeaderElement.BackgroundContent;
        set => HeaderElement.BackgroundContent = value;
    }

    /// <summary>
    /// Gets or sets the content of customized zone before title in header.
    /// </summary>
    public UIElement BeforeTitleContent
    {
        get => HeaderElement.BeforeContent;
        set => HeaderElement.BeforeContent = value;
    }

    /// <summary>
    /// Gets or sets the content of customized zone after title in header.
    /// </summary>
    public UIElement AfterTitleContent
    {
        get => HeaderElement.AfterContent;
        set => HeaderElement.AfterContent = value;
    }

    /// <summary>
    /// Gets or sets the content of customized zone before paging buttons in header.
    /// </summary>
    public UIElement BeforePagingContent
    {
        get => BeforePaging.Child;
        set => BeforePaging.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of customized zone after paging buttons in header.
    /// </summary>
    public UIElement AfterPagingContent
    {
        get => AfterPaging.Child;
        set => AfterPaging.Child = value;
    }

    public TileItem AddItem()
    {
        var item = new TileItem
        {
            Style = ItemStyle
        };
        //item.SetBinding(StyleProperty, new Binding
        //{
        //    Source = this,
        //    Path = "ItemStyle"
        //});
        ListPanel.Children.Add(item);
        return item;
    }

    public TileItem AddItem(Action<TileItem> fillProperties)
    {
        var item = new TileItem
        {
            Style = ItemStyle
        };
        fillProperties?.Invoke(item);
        ListPanel.Children.Add(item);
        return item;
    }

    public TileItem AddItem(string title, Uri image, string description = null)
    {
        var item = new TileItem
        {
            Title = title,
            ImageUri = image,
            Description = description
        };
        item.Style = ItemStyle;
        ListPanel.Children.Add(item);
        return item;
    }

    private void ScrollViewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        => ListScrollView.HorizontalScrollMode = e.Pointer.PointerDeviceType != Microsoft.UI.Input.PointerDeviceType.Mouse ? ScrollMode.Auto : ScrollMode.Disabled;

    private void ScrollViewer_PointerExited(object sender, PointerRoutedEventArgs e)
    { }

    private static void OnItemStyleChanged(TileCollection c, ChangeEventArgs<Style> e, DependencyProperty p)
    {
        foreach (var item in c.ListPanel.Children)
        {
            if (item is not TileItem ti) continue;
            ti.Style = e.NewValue;
        }
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        ListScrollView.ScrollToHorizontalOffset(ListScrollView.HorizontalOffset - ListScrollView.ActualWidth * 3 / 4);
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        ListScrollView.ScrollToHorizontalOffset(ListScrollView.HorizontalOffset + ListScrollView.ActualWidth * 3 / 4);
    }
}
