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
using Trivial.Text;
using Windows.UI.Text;
using Trivial.Data;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<CommentView>;

/// <summary>
/// The comment item view.
/// </summary>
public sealed partial class CommentView : UserControl
{
    /// <summary>
    /// The dependency property of reply stack panel style.
    /// </summary>
    public static readonly DependencyProperty ReplyPanelStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(ReplyPanelStyle), (c, e, d) => c.ReplyPanelStyle = e.NewValue);

    /// <summary>
    /// The dependency property of avatar URI.
    /// </summary>
    public static readonly DependencyProperty AvatarUriProperty = DependencyObjectProxy.RegisterProperty<Uri>(nameof(AvatarUri));

    /// <summary>
    /// The dependency property of nickname.
    /// </summary>
    public static readonly DependencyProperty NicknameProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Nickname), OnTextChanged);

    /// <summary>
    /// The dependency property of publish description.
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Description), OnTextChanged);

    /// <summary>
    /// The dependency property of content text.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Text), OnTextChanged);

    /// <summary>
    /// The dependency property of content margin.
    /// </summary>
    public static readonly DependencyProperty ContentMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(ContentMargin));

    /// <summary>
    /// The dependency property of content height.
    /// </summary>
    public static readonly DependencyProperty ContentHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(ContentHeight));

    /// <summary>
    /// The dependency property of sender information margin.
    /// </summary>
    public static readonly DependencyProperty SenderMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(SenderMargin));

    /// <summary>
    /// The dependency property of sender information orientation.
    /// </summary>
    public static readonly DependencyProperty SenderOrientationProperty = DependencyObjectProxy.RegisterProperty(nameof(SenderOrientation), Orientation.Vertical);

    /// <summary>
    /// The dependency property of sender information trimming.
    /// </summary>
    public static readonly DependencyProperty SenderTrimmingProperty = DependencyObjectProxy.RegisterProperty<TextTrimming>(nameof(SenderTrimming));

    /// <summary>
    /// The dependency property of sender spacing.
    /// </summary>
    public static readonly DependencyProperty SenderSpacingProperty = DependencyObjectProxy.RegisterProperty(nameof(SenderSpacing), 0d);

    /// <summary>
    /// The dependency property of text horiontal text alignment.
    /// </summary>
    public static readonly DependencyProperty HorizontalTextAlignmentProperty = DependencyObjectProxy.RegisterProperty(nameof(HorizontalTextAlignment), TextAlignment.Left);

    /// <summary>
    /// The dependency property of nickname font size.
    /// </summary>
    public static readonly DependencyProperty NicknameFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(NicknameFontSize), 12d);

    /// <summary>
    /// The dependency property of nickname font weight.
    /// </summary>
    public static readonly DependencyProperty NicknameFontWeightProperty = DependencyObjectProxy.RegisterFontWeightProperty(nameof(NicknameFontWeight));

    /// <summary>
    /// The dependency property of nickname font style.
    /// </summary>
    public static readonly DependencyProperty NicknameFontStyleProperty = DependencyObjectProxy.RegisterProperty<FontStyle>(nameof(NicknameFontStyle));

    /// <summary>
    /// The dependency property of nickname line height.
    /// </summary>
    public static readonly DependencyProperty NicknameLineHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(NicknameLineHeight), 0d);

    /// <summary>
    /// The dependency property of nickname foreground.
    /// </summary>
    public static readonly DependencyProperty NicknameForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(NicknameForeground));

    /// <summary>
    /// The dependency property of nickname width.
    /// </summary>
    public static readonly DependencyProperty NicknameWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(NicknameWidth));

    /// <summary>
    /// The dependency property of nickname height.
    /// </summary>
    public static readonly DependencyProperty NicknameHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(NicknameHeight));

    /// <summary>
    /// The dependency property of description font size.
    /// </summary>
    public static readonly DependencyProperty DescriptionFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(DescriptionFontSize), 11d);

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
    /// The dependency property of description width.
    /// </summary>
    public static readonly DependencyProperty DescriptionWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionWidth));

    /// <summary>
    /// The dependency property of description height.
    /// </summary>
    public static readonly DependencyProperty DescriptionHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(DescriptionHeight));

    /// <summary>
    /// The dependency property of avatar image width.
    /// </summary>
    public static readonly DependencyProperty AvatarWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(AvatarWidth));

    /// <summary>
    /// The dependency property of avatar image height.
    /// </summary>
    public static readonly DependencyProperty AvatarHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(AvatarHeight));

    /// <summary>
    /// The dependency property of avatar image background.
    /// </summary>
    public static readonly DependencyProperty AvatarBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(AvatarBackground));

    /// <summary>
    /// The dependency property of avatar image background sizing.
    /// </summary>
    public static readonly DependencyProperty AvatarBackgroundSizingProperty = DependencyObjectProxy.RegisterProperty<BackgroundSizing>(nameof(AvatarBackgroundSizing));

    /// <summary>
    /// The dependency property of avatar image margin.
    /// </summary>
    public static readonly DependencyProperty AvatarMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(AvatarMargin));

    /// <summary>
    /// The dependency property of avatar image corner radius.
    /// </summary>
    public static readonly DependencyProperty AvatarCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(AvatarCornerRadius));

    /// <summary>
    /// The dependency property of reply visibility.
    /// </summary>
    public static readonly DependencyProperty ReplyVisibilityProperty = DependencyObjectProxy.RegisterProperty<Visibility>(nameof(ReplyVisibility), Visibility.Collapsed);

    /// <summary>
    /// Initializes a new instance of the CommentView class.
    /// </summary>
    public CommentView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the avatar URI of sender.
    /// </summary>
    public Uri AvatarUri
    {
        get => (Uri)GetValue(AvatarUriProperty);
        set => SetValue(AvatarUriProperty, value);
    }

    /// <summary>
    /// Gets or sets the nickname of sender.
    /// </summary>
    public string Nickname
    {
        get => (string)GetValue(NicknameProperty);
        set => SetValue(NicknameProperty, value);
    }

    /// <summary>
    /// Gets or sets the publish description.
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the content text.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of content.
    /// </summary>
    public double ContentHeight
    {
        get => (double)GetValue(ContentHeightProperty);
        set => SetValue(ContentHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin of content.
    /// </summary>
    public Thickness ContentMargin
    {
        get => (Thickness)GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin of sender information.
    /// </summary>
    public Thickness SenderMargin
    {
        get => (Thickness)GetValue(SenderMarginProperty);
        set => SetValue(SenderMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between each sender information.
    /// </summary>
    public double SenderSpacing
    {
        get => (double)GetValue(SenderSpacingProperty);
        set => SetValue(SenderSpacingProperty, value);
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
    /// Gets or sets the orientation of the image and text.
    /// </summary>
    public Orientation SenderOrientation
    {
        get => (Orientation)GetValue(SenderOrientationProperty);
        set => SetValue(SenderOrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the trimming mode of title.
    /// </summary>
    public TextTrimming SenderTrimming
    {
        get => (TextTrimming)GetValue(SenderTrimmingProperty);
        set => SetValue(SenderTrimmingProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size of nickname.
    /// </summary>
    public double NicknameFontSize
    {
        get => (double)GetValue(NicknameFontSizeProperty);
        set => SetValue(NicknameFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the font weight of nickname.
    /// </summary>
    public FontWeight NicknameFontWeight
    {
        get => (FontWeight)GetValue(NicknameFontWeightProperty);
        set => SetValue(NicknameFontWeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the font style of nickname.
    /// </summary>
    public FontStyle NicknameFontStyle
    {
        get => (FontStyle)GetValue(NicknameFontStyleProperty);
        set => SetValue(NicknameFontStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the line height of nickname.
    /// </summary>
    public double NicknameLineHeight
    {
        get => (double)GetValue(NicknameLineHeightProperty);
        set => SetValue(NicknameLineHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground of nickname.
    /// </summary>
    public Brush NicknameForeground
    {
        get => (Brush)GetValue(NicknameForegroundProperty);
        set => SetValue(NicknameForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of nickname.
    /// </summary>
    public double NicknameWidth
    {
        get => (double)GetValue(NicknameWidthProperty);
        set => SetValue(NicknameWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of nickname.
    /// </summary>
    public double NicknameHeight
    {
        get => (double)GetValue(NicknameHeightProperty);
        set => SetValue(NicknameHeightProperty, value);
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
    /// Gets or sets the width of avatar image.
    /// </summary>
    public double AvatarWidth
    {
        get => (double)GetValue(AvatarWidthProperty);
        set => SetValue(AvatarWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of avatar image.
    /// </summary>
    public double AvatarHeight
    {
        get => (double)GetValue(AvatarHeightProperty);
        set => SetValue(AvatarHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the background of avatar image.
    /// </summary>
    public Brush AvatarBackground
    {
        get => (Brush)GetValue(AvatarBackgroundProperty);
        set => SetValue(AvatarBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of avatar image.
    /// </summary>
    public CornerRadius AvatarCornerRadius
    {
        get => (CornerRadius)GetValue(AvatarCornerRadiusProperty);
        set => SetValue(AvatarCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the background sizing of avatar image.
    /// </summary>
    public BackgroundSizing AvatarBackgroundSizing
    {
        get => (BackgroundSizing)GetValue(AvatarBackgroundSizingProperty);
        set => SetValue(AvatarBackgroundSizingProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin of avatar image.
    /// </summary>
    public Thickness AvatarMargin
    {
        get => (Thickness)GetValue(AvatarMarginProperty);
        set => SetValue(AvatarMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the visibility of reply panel.
    /// </summary>
    public Visibility ReplyVisibility
    {
        get => (Visibility)GetValue(ReplyVisibilityProperty);
        set => SetValue(ReplyVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of reply stack panel.
    /// </summary>
    public Style ReplyPanelStyle
    {
        get => (Style)GetValue(ReplyPanelStyleProperty);
        set => SetValue(ReplyPanelStyleProperty, value);
    }

    /// <summary>
    /// Gets the children of reply stack panel.
    /// </summary>
    public UIElementCollection ReplyChildren => ReplayPanel.Children;

    /// <summary>
    /// Gets or sets the content of child.
    /// </summary>
    public UIElement Child
    {
        get => ContentPanel.Child;
        set => ContentPanel.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of additional sender information.
    /// </summary>
    public UIElement SenderAdditionalContent
    {
        get => SenderAdditionalInfo.Child;
        set => SenderAdditionalInfo.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of before customized zone.
    /// </summary>
    public UIElement BeforeContent
    {
        get => BeforeContentPanel.Child;
        set => BeforeContentPanel.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of after customized zone.
    /// </summary>
    public UIElement AfterContent
    {
        get => AfterContentPanel.Child;
        set => AfterContentPanel.Child = value;
    }

    /// <summary>
    /// Sets publish date as description
    /// </summary>
    /// <param name="date">The date time to publish.</param>
    /// <returns>The result of description.</returns>
    public string SetPublishDate(DateTime date)
        => Description = date.Date == DateTime.Now.Date ? date.ToShortTimeString() : date.ToShortDateString();
    
    private static void OnTextChanged(CommentView c, ChangeEventArgs<string> e, DependencyProperty p)
    {
        c.NicknameText.Visibility = string.IsNullOrWhiteSpace(c.Nickname) ? Visibility.Collapsed : Visibility.Visible;
        c.DescriptionText.Visibility = string.IsNullOrWhiteSpace(c.Description) ? Visibility.Collapsed : Visibility.Visible;
        c.ContentText.Visibility = string.IsNullOrWhiteSpace(c.Text) ? Visibility.Collapsed : Visibility.Visible;
    }
}
