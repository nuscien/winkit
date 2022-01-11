using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
using Windows.UI.Text;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<TileItem>;

/// <summary>
/// The tile item control.
/// </summary>
public sealed partial class TileItem : UserControl
{
    public static readonly DependencyProperty HoverForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverForeground));
    public static readonly DependencyProperty PressedForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(PressedForeground));
    public static readonly DependencyProperty HoverBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverBackground));
    public static readonly DependencyProperty PressedBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(PressedBackground));
    public static readonly DependencyProperty OrientationProperty = DependencyObjectProxy.RegisterProperty(nameof(Orientation), Orientation.Vertical);
    public static readonly DependencyProperty TextWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TextWidth));
    public static readonly DependencyProperty TextHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TextHeight));
    public static readonly DependencyProperty TextBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TextBackground));
    public static readonly DependencyProperty TextPaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(TextPadding));
    public static readonly DependencyProperty TextMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(TextMargin));
    public static readonly DependencyProperty TextCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(TextCornerRadius));
    public static readonly DependencyProperty TitleProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Title));
    public static readonly DependencyProperty TitleFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleFontSize), 14d);
    public static readonly DependencyProperty TitleFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(TitleFontWeight));
    public static readonly DependencyProperty TitleFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(TitleFontStyle));
    public static readonly DependencyProperty TitleLineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleLineHeight), 0d);
    public static readonly DependencyProperty TitleForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(TitleForeground));
    public static readonly DependencyProperty TitleWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TitleWidth));
    public static readonly DependencyProperty TitleHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(TitleHeight));
    public static readonly DependencyProperty TitleWrappingProperty = DependencyObjectProxy.RegisterProperty<TextWrapping>(nameof(TitleWrapping));
    public static readonly DependencyProperty TitleTrimmingProperty = DependencyObjectProxy.RegisterProperty<TextTrimming>(nameof(TitleTrimming));
    public static readonly DependencyProperty DescriptionProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Description));
    public static readonly DependencyProperty DescriptionFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(DescriptionFontSize), 12d);
    public static readonly DependencyProperty DescriptionFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(DescriptionFontWeight));
    public static readonly DependencyProperty DescriptionFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(DescriptionFontStyle));
    public static readonly DependencyProperty DescriptionLineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(DescriptionLineHeight), 0d);
    public static readonly DependencyProperty DescriptionForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DescriptionForeground));
    public static readonly DependencyProperty DescriptionWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionWidth));
    public static readonly DependencyProperty DescriptionHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionHeight));
    public static readonly DependencyProperty DescriptionWrappingProperty = DependencyObjectProxy.RegisterProperty<TextWrapping>(nameof(DescriptionWrapping));
    public static readonly DependencyProperty DescriptionTrimmingProperty = DependencyObjectProxy.RegisterProperty<TextTrimming>(nameof(DescriptionTrimming));
    public static readonly DependencyProperty ImageUriProperty = DependencyObjectProxy.RegisterProperty<Uri>(nameof(ImageUri), OnImageUriChanged);
    //public static readonly DependencyProperty ImageSourceProperty = DependencyObjectProxy.RegisterProperty<ImageSource>(nameof(ImageSource));
    public static readonly DependencyProperty ImageWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(ImageWidth));
    public static readonly DependencyProperty ImageHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(ImageHeight));
    public static readonly DependencyProperty ImageBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ImageBackground));
    public static readonly DependencyProperty ImagePaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(ImagePadding));
    public static readonly DependencyProperty ImageMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(ImageMargin));
    public static readonly DependencyProperty ImageCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(ImageCornerRadius));

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
    /// Gets or sets a value that indicates when the Click event occurs, in terms of device behavior.
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

    private Visibility NeedShowTitle => string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;

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
    /// Gets or sets the description.
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    private Visibility NeedShowDescription => string.IsNullOrEmpty(Description) ? Visibility.Collapsed : Visibility.Visible;

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

    ///// <summary>
    ///// Gets or sets the image source.
    ///// </summary>
    //public ImageSource ImageSource
    //{
    //    get => (ImageSource)GetValue(ImageSourceProperty);
    //    set => SetValue(ImageSourceProperty, value);
    //}

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
    /// Gets or sets the corner radius of image.
    /// </summary>
    public CornerRadius ImageCornerRadius
    {
        get => (CornerRadius)GetValue(ImageCornerRadiusProperty);
        set => SetValue(ImageCornerRadiusProperty, value);
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

    private void Button_Click(object sender, RoutedEventArgs e)
        => Click?.Invoke(this, e);

    private void OwnerButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
    }

    private void OwnerButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
    }

    private static void OnImageUriChanged(TileItem c, ChangeEventArgs<Uri> e, DependencyProperty p)
    {
        //if (e == null)
        //{
        //    c.ImageSource = null;
        //    return;
        //}

        //c.ImageSource = new BitmapImage(e.NewValue);
    }
}
