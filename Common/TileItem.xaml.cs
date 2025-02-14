﻿using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Trivial.Data;
using Trivial.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<TileItem>;

/// <summary>
/// The tile item control.
/// </summary>
public sealed partial class TileItem : UserControl
{
    /// <summary>
    /// The dependency property of click mode.
    /// </summary>
    public static readonly DependencyProperty ClickModeProperty = DependencyObjectProxy.RegisterProperty<ClickMode>(nameof(ClickMode), (c, e, p) => c.ClickMode = e.NewValue);

    /// <summary>
    /// The dependency property of hover foreground.
    /// </summary>
    public static readonly DependencyProperty HoverForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverForeground));
    
    /// <summary>
    /// The dependency property of pressed foreground.
    /// </summary>
    public static readonly DependencyProperty PressedForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(PressedForeground));
    
    /// <summary>
    /// The dependency property of hover background.
    /// </summary>
    public static readonly DependencyProperty HoverBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverBackground));

    /// <summary>
    /// The dependency property of pressed background.
    /// </summary>
    public static readonly DependencyProperty PressedBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(PressedBackground));

    /// <summary>
    /// The dependency property of orientation.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty = DependencyObjectProxy.RegisterProperty(nameof(Orientation), Orientation.Vertical);

    /// <summary>
    /// The dependency property of text width.
    /// </summary>
    public static readonly DependencyProperty TextWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TextWidth));

    /// <summary>
    /// The dependency property of text height.
    /// </summary>
    public static readonly DependencyProperty TextHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TextHeight));

    /// <summary>
    /// The dependency property of horiontal text alignment.
    /// </summary>
    public static readonly DependencyProperty HorizontalTextAlignmentProperty = DependencyObjectProxy.RegisterProperty(nameof(HorizontalTextAlignment), TextAlignment.Left);

    /// <summary>
    /// The dependency property of text background.
    /// </summary>
    public static readonly DependencyProperty TextBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TextBackground));

    /// <summary>
    /// The dependency property of text background sizing.
    /// </summary>
    public static readonly DependencyProperty TextBackgroundSizingProperty = DependencyObjectProxy.RegisterProperty<BackgroundSizing>(nameof(TextBackgroundSizing));

    /// <summary>
    /// The dependency property of text padding.
    /// </summary>
    public static readonly DependencyProperty TextPaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(TextPadding));

    /// <summary>
    /// The dependency property of text margin.
    /// </summary>
    public static readonly DependencyProperty TextMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(TextMargin));

    /// <summary>
    /// The dependency property of text border brush.
    /// </summary>
    public static readonly DependencyProperty TextBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TextBorderBrush));

    /// <summary>
    /// The dependency property of text thickness brush.
    /// </summary>
    public static readonly DependencyProperty TextBorderThicknessProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(TextBorderThickness));

    /// <summary>
    /// The dependency property of text corner radius.
    /// </summary>
    public static readonly DependencyProperty TextCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(TextCornerRadius));

    /// <summary>
    /// The dependency property of title property.
    /// </summary>
    public static readonly DependencyProperty TitleProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Title), OnTextChanged);

    /// <summary>
    /// The dependency property of title font size.
    /// </summary>
    public static readonly DependencyProperty TitleFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleFontSize), 14d);

    /// <summary>
    /// The dependency property of title font weight.
    /// </summary>
    public static readonly DependencyProperty TitleFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(TitleFontWeight));

    /// <summary>
    /// The dependency property of title font style.
    /// </summary>
    public static readonly DependencyProperty TitleFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(TitleFontStyle));

    /// <summary>
    /// The dependency property of title line height.
    /// </summary>
    public static readonly DependencyProperty TitleLineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleLineHeight), 0d);

    /// <summary>
    /// The dependency property of title foreground.
    /// </summary>
    public static readonly DependencyProperty TitleForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TitleForeground), new SolidColorBrush(Microsoft.UI.Colors.Gray));

    /// <summary>
    /// The dependency property of title width.
    /// </summary>
    public static readonly DependencyProperty TitleWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TitleWidth));

    /// <summary>
    /// The dependency property of title height.
    /// </summary>
    public static readonly DependencyProperty TitleHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TitleHeight));

    /// <summary>
    /// The dependency property of title max height.
    /// </summary>
    public static readonly DependencyProperty TitleMaxHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleMaxHeight), double.PositiveInfinity);

    /// <summary>
    /// The dependency property of title horizontal alignment property.
    /// </summary>
    public static readonly DependencyProperty TitleHorizontalAlignmentProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleHorizontalAlignment), HorizontalAlignment.Stretch);

    /// <summary>
    /// The dependency property of title wrapping.
    /// </summary>
    public static readonly DependencyProperty TitleWrappingProperty = DependencyObjectProxy.RegisterProperty<TextWrapping>(nameof(TitleWrapping));

    /// <summary>
    /// The dependency property of title trimming.
    /// </summary>
    public static readonly DependencyProperty TitleTrimmingProperty = DependencyObjectProxy.RegisterProperty<TextTrimming>(nameof(TitleTrimming));

    /// <summary>
    /// The dependency property of title connected animation key property.
    /// </summary>
    public static readonly DependencyProperty TitleConnectedAnimationKeyProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(TitleConnectedAnimationKey));

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
    public static readonly DependencyProperty DescriptionForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DescriptionForeground), new SolidColorBrush(Microsoft.UI.Colors.Gray));

    /// <summary>
    /// The dependency property of description width.
    /// </summary>
    public static readonly DependencyProperty DescriptionWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionWidth));

    /// <summary>
    /// The dependency property of description height.
    /// </summary>
    public static readonly DependencyProperty DescriptionHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionHeight));

    /// <summary>
    /// The dependency property of description horizontal alignment property.
    /// </summary>
    public static readonly DependencyProperty DescriptionHorizontalAlignmentProperty = DependencyObjectProxy.RegisterProperty(nameof(DescriptionHorizontalAlignment), HorizontalAlignment.Stretch);

    /// <summary>
    /// The dependency property of description wrapping.
    /// </summary>
    public static readonly DependencyProperty DescriptionWrappingProperty = DependencyObjectProxy.RegisterProperty<TextWrapping>(nameof(DescriptionWrapping));

    /// <summary>
    /// The dependency property of description trimming.
    /// </summary>
    public static readonly DependencyProperty DescriptionTrimmingProperty = DependencyObjectProxy.RegisterProperty<TextTrimming>(nameof(DescriptionTrimming));

    /// <summary>
    /// The dependency property of image URI.
    /// </summary>
    public static readonly DependencyProperty ImageUriProperty = DependencyObjectProxy.RegisterProperty<Uri>(nameof(ImageUri), OnImageUriChanged);

    /// <summary>
    /// The dependency property of image width.
    /// </summary>
    public static readonly DependencyProperty ImageWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(ImageWidth));

    /// <summary>
    /// The dependency property of image height.
    /// </summary>
    public static readonly DependencyProperty ImageHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(ImageHeight));

    /// <summary>
    /// The dependency property of image background.
    /// </summary>
    public static readonly DependencyProperty ImageBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ImageBackground));

    /// <summary>
    /// The dependency property of image background sizing.
    /// </summary>
    public static readonly DependencyProperty ImageBackgroundSizingProperty = DependencyObjectProxy.RegisterProperty<BackgroundSizing>(nameof(ImageBackgroundSizing));

    /// <summary>
    /// The dependency property of image padding.
    /// </summary>
    public static readonly DependencyProperty ImagePaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(ImagePadding));

    /// <summary>
    /// The dependency property of image margin.
    /// </summary>
    public static readonly DependencyProperty ImageMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(ImageMargin));

    /// <summary>
    /// The dependency property of image border brush.
    /// </summary>
    public static readonly DependencyProperty ImageBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ImageBorderBrush));

    /// <summary>
    /// The dependency property of image thickness brush.
    /// </summary>
    public static readonly DependencyProperty ImageBorderThicknessProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(ImageBorderThickness));

    /// <summary>
    /// The dependency property of image corner radius.
    /// </summary>
    public static readonly DependencyProperty ImageCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(ImageCornerRadius));

    /// <summary>
    /// The dependency property of image horizontal alignment property.
    /// </summary>
    public static readonly DependencyProperty ImageHorizontalAlignmentProperty = DependencyObjectProxy.RegisterProperty(nameof(ImageHorizontalAlignment), HorizontalAlignment.Stretch);

    /// <summary>
    /// The dependency property of image connected animation key property.
    /// </summary>
    public static readonly DependencyProperty ImageConnectedAnimationKeyProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(ImageConnectedAnimationKey));

    private Uri imageUri;

    /// <summary>
    /// Initializes a new instance of the TileItem class.
    /// </summary>
    public TileItem()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Occurs when this control is clicked.
    /// </summary>
    public event RoutedEventHandler Click;

    /// <summary>
    /// Occurs when the title trimmed property value has changed.
    /// </summary>
    public event TypedEventHandler<TileItem, IsTextTrimmedChangedEventArgs> IsTitleTrimmedChanged;

    /// <summary>
    /// Occurs when the description trimmed property value has changed.
    /// </summary>
    public event TypedEventHandler<TileItem, IsTextTrimmedChangedEventArgs> IsDescriptionTrimmedChanged;

    /// <summary>
    /// Occurs when there is an error associated with image retrieval or format.
    /// </summary>
    public event ExceptionRoutedEventHandler ImageFailed;

    /// <summary>
    /// Occurs when the image source is downloaded and decoded with no failure.
    /// </summary>
    public event RoutedEventHandler ImageOpened;

    /// <summary>
    /// Gets or sets the button style.
    /// </summary>
    public Style ButtonStyle
    {
        get => OwnerButton.Style;
        set => OwnerButton.Style = value;
    }

    /// <summary>
    /// Gets or sets the button template.
    /// </summary>
    public ControlTemplate ButtonTemplate
    {
        get => OwnerButton.Template;
        set => OwnerButton.Template = value;
    }

    /// <summary>
    /// Gets or sets the hover foreground.
    /// </summary>
    public Brush HoverForeground
    {
        get => (Brush)GetValue(HoverForegroundProperty);
        set => SetValue(HoverForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the pressed foreground.
    /// </summary>
    public Brush PressedForeground
    {
        get => (Brush)GetValue(PressedForegroundProperty);
        set => SetValue(PressedForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the hover background.
    /// </summary>
    public Brush HoverBackground
    {
        get => (Brush)GetValue(HoverBackgroundProperty);
        set => SetValue(HoverBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the pressed background.
    /// </summary>
    public Brush PressedBackground
    {
        get => (Brush)GetValue(PressedBackgroundProperty);
        set => SetValue(PressedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the image and text.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates when the click event occurs, in terms of device behavior.
    /// </summary>
    public ClickMode ClickMode
    {
        get => OwnerButton.ClickMode;
        set => OwnerButton.ClickMode = value;
    }

    /// <summary>
    /// Gets or sets the background of text zone.
    /// </summary>
    public Brush TextBackground
    {
        get => (Brush)GetValue(TextBackgroundProperty);
        set => SetValue(TextBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background sizing of text zone.
    /// </summary>
    public BackgroundSizing TextBackgroundSizing
    {
        get => (BackgroundSizing)GetValue(TextBackgroundSizingProperty);
        set => SetValue(TextBackgroundSizingProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of text zone.
    /// </summary>
    public double TextWidth
    {
        get => (double)GetValue(TextWidthProperty);
        set => SetValue(TextWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of text zone.
    /// </summary>
    public double TextHeight
    {
        get => (double)GetValue(TextHeightProperty);
        set => SetValue(TextHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the alignment of text zone.
    /// </summary>
    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding of text zone.
    /// </summary>
    public Thickness TextPadding
    {
        get => (Thickness)GetValue(TextPaddingProperty);
        set => SetValue(TextPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin of text zone.
    /// </summary>
    public Thickness TextMargin
    {
        get => (Thickness)GetValue(TextMarginProperty);
        set => SetValue(TextMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the border brush of text zone.
    /// </summary>
    public Brush TextBorderBrush
    {
        get => (Brush)GetValue(TextBorderBrushProperty);
        set => SetValue(TextBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the border thickness of text zone.
    /// </summary>
    public Thickness TextBorderThickness
    {
        get => (Thickness)GetValue(TextBorderThicknessProperty);
        set => SetValue(TextBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of text zone.
    /// </summary>
    public CornerRadius TextCornerRadius
    {
        get => (CornerRadius)GetValue(TextCornerRadiusProperty);
        set => SetValue(TextCornerRadiusProperty, value);
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
    /// Gets or sets the line height of title.
    /// </summary>
    public double TitleLineHeight
    {
        get => (double)GetValue(TitleLineHeightProperty);
        set => SetValue(TitleLineHeightProperty, value);
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
    /// Gets or sets the width of title.
    /// </summary>
    public double TitleWidth
    {
        get => (double)GetValue(TitleWidthProperty);
        set => SetValue(TitleWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of title.
    /// </summary>
    public double TitleHeight
    {
        get => (double)GetValue(TitleHeightProperty);
        set => SetValue(TitleHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the max height of title.
    /// </summary>
    public double TitleMaxHeight
    {
        get => (double)GetValue(TitleMaxHeightProperty);
        set => SetValue(TitleMaxHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of title.
    /// </summary>
    public HorizontalAlignment TitleHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(TitleHorizontalAlignmentProperty);
        set => SetValue(TitleHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the wrapping mode of title.
    /// </summary>
    public TextWrapping TitleWrapping
    {
        get => (TextWrapping)GetValue(TitleWrappingProperty);
        set => SetValue(TitleWrappingProperty, value);
    }

    /// <summary>
    /// Gets or sets the trimming mode of title.
    /// </summary>
    public TextTrimming TitleTrimming
    {
        get => (TextTrimming)GetValue(TitleTrimmingProperty);
        set => SetValue(TitleTrimmingProperty, value);
    }

    /// <summary>
    /// Gets or sets the title connected animation key.
    /// </summary>
    public string TitleConnectedAnimationKey
    {
        get => (string)GetValue(TitleConnectedAnimationKeyProperty);
        set => SetValue(TitleConnectedAnimationKeyProperty, value);
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
    /// Gets or sets the width of description.
    /// </summary>
    public double DescriptionWidth
    {
        get => (double)GetValue(DescriptionWidthProperty);
        set => SetValue(DescriptionWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of description.
    /// </summary>
    public double DescriptionHeight
    {
        get => (double)GetValue(DescriptionHeightProperty);
        set => SetValue(DescriptionHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of description.
    /// </summary>
    public HorizontalAlignment DescriptionHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(DescriptionHorizontalAlignmentProperty);
        set => SetValue(DescriptionHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the wrapping mode of description.
    /// </summary>
    public TextWrapping DescriptionWrapping
    {
        get => (TextWrapping)GetValue(DescriptionWrappingProperty);
        set => SetValue(DescriptionWrappingProperty, value);
    }

    /// <summary>
    /// Gets or sets the trimming mode of description.
    /// </summary>
    public TextTrimming DescriptionTrimming
    {
        get => (TextTrimming)GetValue(DescriptionTrimmingProperty);
        set => SetValue(DescriptionTrimmingProperty, value);
    }

    /// <summary>
    /// Gets or sets the image URI.
    /// </summary>
    public Uri ImageUri
    {
        get => (Uri)GetValue(ImageUriProperty);
        set => SetValue(ImageUriProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of image.
    /// </summary>
    public double ImageWidth
    {
        get => (double)GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of image.
    /// </summary>
    public double ImageHeight
    {
        get => (double)GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the background of image.
    /// </summary>
    public Brush ImageBackground
    {
        get => (Brush)GetValue(ImageBackgroundProperty);
        set => SetValue(ImageBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background sizing of image.
    /// </summary>
    public BackgroundSizing ImageBackgroundSizing
    {
        get => (BackgroundSizing)GetValue(ImageBackgroundSizingProperty);
        set => SetValue(ImageBackgroundSizingProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding of image.
    /// </summary>
    public Thickness ImagePadding
    {
        get => (Thickness)GetValue(ImagePaddingProperty);
        set => SetValue(ImagePaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin of image.
    /// </summary>
    public Thickness ImageMargin
    {
        get => (Thickness)GetValue(ImageMarginProperty);
        set => SetValue(ImageMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the border brush of image.
    /// </summary>
    public Brush ImageBorderBrush
    {
        get => (Brush)GetValue(ImageBorderBrushProperty);
        set => SetValue(ImageBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the border thickness of image.
    /// </summary>
    public Thickness ImageBorderThickness
    {
        get => (Thickness)GetValue(ImageBorderThicknessProperty);
        set => SetValue(ImageBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of image.
    /// </summary>
    public CornerRadius ImageCornerRadius
    {
        get => (CornerRadius)GetValue(ImageCornerRadiusProperty);
        set => SetValue(ImageCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the image connected animation key.
    /// </summary>
    public string ImageConnectedAnimationKey
    {
        get => (string)GetValue(ImageConnectedAnimationKeyProperty);
        set => SetValue(ImageConnectedAnimationKeyProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of image.
    /// </summary>
    public HorizontalAlignment ImageHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(ImageHorizontalAlignmentProperty);
        set => SetValue(ImageHorizontalAlignmentProperty, value);
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
    /// Gets or sets the content of image cover zone.
    /// </summary>
    public UIElement ImageCoverContent
    {
        get => ImageCover.Child;
        set => ImageCover.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of text cover zone.
    /// </summary>
    public UIElement TextCoverContent
    {
        get => TextCover.Child;
        set => TextCover.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of image cover zone.
    /// </summary>
    public UIElement ImageBackgroundContent
    {
        get => ImageBack.Child;
        set => ImageBack.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of text cover zone.
    /// </summary>
    public UIElement TextBackgroundContent
    {
        get => TextBack.Child;
        set => TextBack.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of customized zone before text.
    /// </summary>
    public UIElement BeforeTextContent
    {
        get => BeforeText.Child;
        set => BeforeText.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of customized zone after text.
    /// </summary>
    public UIElement AfterTextContent
    {
        get => AfterText.Child;
        set => AfterText.Child = value;
    }

    /// <summary>
    /// Gets the collection of title highlights.
    /// </summary>
    public IList<Microsoft.UI.Xaml.Documents.TextHighlighter> TitleHighlighters => TitleText.TextHighlighters;

    /// <summary>
    /// Gets the collection of description highlights.
    /// </summary>
    public IList<Microsoft.UI.Xaml.Documents.TextHighlighter> DescriptionHighlighters => DescriptionText.TextHighlighters;

    /// <summary>
    /// Gets a value by which each line of title text content is offset from a baseline.
    /// </summary>
    public double TitleBaselineOffset => TitleText.BaselineOffset;

    /// <summary>
    /// Gets a value by which each line of description text content is offset from a baseline.
    /// </summary>
    public double DescriptionBaselineOffset => DescriptionText.BaselineOffset;

    /// <summary>
    /// Gets a value that indicates whether a device pointer is located over this.
    /// </summary>
    public bool IsPointerOver => OwnerButton.IsPointerOver;

    /// <summary>
    /// Gets a value that indicates whether this is currently in a pressed state.
    /// </summary>
    public bool IsPressed => OwnerButton.IsPressed;

    /// <summary>
    /// Sets image URI.
    /// </summary>
    /// <param name="uri">The image URI.</param>
    /// <param name="prepareOnly">true if it is used to prepare to set, that means it will not take effect in fact; otherwise, false.</param>
    public void SetImage(Uri uri, bool prepareOnly = false)
    {
        if (prepareOnly) imageUri = uri;
        else ImageUri = uri;
    }

    /// <summary>
    /// Uses prepare image URI if available.
    /// </summary>
    public void UseImageUriPrepared()
    {
        if (imageUri == null) return;
        ImageUri = imageUri;
    }

    /// <summary>
    /// Sets the value of the ToolTipService.ToolTip XAML attached property.
    /// </summary>
    /// <param name="value">The value to set for tooltip content.</param>
    public void SetTitleToolTip(object value)
        => ToolTipService.SetToolTip(TitleText, value);

    /// <summary>
    /// Sets the value of the ToolTipService.ToolTip XAML attached property.
    /// </summary>
    /// <param name="value">The value to set for tooltip content.</param>
    public void SetDescriptionToolTip(object value)
        => ToolTipService.SetToolTip(DescriptionText, value);

    /// <summary>
    /// Sets the value of the ToolTipService.ToolTip XAML attached property.
    /// </summary>
    /// <param name="value">The value to set for tooltip content.</param>
    public void SetImageToolTip(object value)
        => ToolTipService.SetToolTip(ImageControl, value);

    /// <inheritdoc />
    public override string ToString()
        => Title ?? string.Empty;

    private void TitleText_IsTextTrimmedChanged(TextBlock sender, IsTextTrimmedChangedEventArgs args)
    => IsTitleTrimmedChanged?.Invoke(this, args);

    private void DescriptionText_IsTextTrimmedChanged(TextBlock sender, IsTextTrimmedChangedEventArgs args)
        => IsDescriptionTrimmedChanged?.Invoke(this, args);

    private void ImageControl_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        => ImageFailed?.Invoke(this, e);

    private void ImageControl_ImageOpened(object sender, RoutedEventArgs e)
        => ImageOpened?.Invoke(this, e);

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(TitleConnectedAnimationKey)) ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(TitleConnectedAnimationKey, TitleText);
            if (!string.IsNullOrWhiteSpace(ImageConnectedAnimationKey)) ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(ImageConnectedAnimationKey, ImageControl);
        }
        catch (NullReferenceException)
        {
        }
        catch (InvalidOperationException)
        {
        }

        Click?.Invoke(this, e);
    }

    private static void OnTextChanged(TileItem c, ChangeEventArgs<string> e, DependencyProperty p)
    {
        c.TitleText.Visibility = string.IsNullOrWhiteSpace(c.Title) ? Visibility.Collapsed : Visibility.Visible;
        c.DescriptionText.Visibility = string.IsNullOrWhiteSpace(c.Description) ? Visibility.Collapsed : Visibility.Visible;
    }

    private static void OnImageUriChanged(TileItem c, ChangeEventArgs<Uri> e, DependencyProperty p)
    {
        c.imageUri = null;
    }

    /// <summary>
    /// Creates a tile item.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="image">The image URI.</param>
    /// <param name="description">The description.</param>
    /// <returns>A tile item.</returns>
    public static TileItem Create(string title, Uri image, string description)
        => new()
        {
            Title = title,
            ImageUri = image,
            Description = description
        };

    /// <summary>
    /// Creates a tile item.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>A tile item.</returns>
    public static TileItem Create(BaseItemModel model)
    {
        if (model == null) return null;
        var item = new TileItem
        {
            Title = model.Name,
            ImageUri = model.ImageUri,
            Description = model.Description,
            DataContext = model
        };
        if (model is BaseActiveItemModel active) item.Click += active.ProcessRouted;
        return item;
    }
}
