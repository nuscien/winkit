using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Trivial.Data;
using Trivial.Tasks;
using Trivial.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<TextButton>;

/// <summary>
/// The common basic button with styled states.
/// </summary>
public sealed partial class TextButton : UserControl
{
    /// <summary>
    /// The dependency property of click mode.
    /// </summary>
    public static readonly DependencyProperty ClickModeProperty = DependencyObjectProxy.RegisterProperty<ClickMode>(nameof(ClickMode), (c, e, p) => c.ClickMode = e.NewValue);

    /// <summary>
    /// The dependency property of command.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyObjectProxy.RegisterProperty<ICommand>(nameof(Command), (c, e, p) => c.Command = e.NewValue);

    /// <summary>
    /// The dependency property of command parameter.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyObjectProxy.RegisterProperty<object>(nameof(CommandParameter), (c, e, p) => c.CommandParameter = e.NewValue);

    /// <summary>
    /// The dependency property of additional state property.
    /// </summary>
    public static readonly DependencyProperty AdditionalStateProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(AdditionalState));

    /// <summary>
    /// The dependency property of text property.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(Text));

    /// <summary>
    /// The dependency property of horiontal text alignment.
    /// </summary>
    public static readonly DependencyProperty HorizontalTextAlignmentProperty = DependencyObjectProxy.RegisterProperty(nameof(HorizontalTextAlignment), TextAlignment.Left);

    /// <summary>
    /// The dependency property of orientation.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty = DependencyObjectProxy.RegisterProperty(nameof(Orientation), Orientation.Horizontal);

    /// <summary>
    /// The dependency property of spacing.
    /// </summary>
    public static readonly DependencyProperty SpacingProperty = DependencyObjectProxy.RegisterProperty(nameof(Spacing), OnSpacingChanged, 0d);

    /// <summary>
    /// The dependency property of hover foreground.
    /// </summary>
    public static readonly DependencyProperty HoverForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverForeground));

    /// <summary>
    /// The dependency property of disabled foreground.
    /// </summary>
    public static readonly DependencyProperty DisabledForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DisabledForeground));

    /// <summary>
    /// The dependency property of base background.
    /// </summary>
    public static readonly DependencyProperty NormalBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(NormalBackground));

    /// <summary>
    /// The dependency property of hover background.
    /// </summary>
    public static readonly DependencyProperty HoverBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverBackground));

    /// <summary>
    /// The dependency property of disabled background.
    /// </summary>
    public static readonly DependencyProperty DisabledBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DisabledBackground));

    /// <summary>
    /// The dependency property of normal border brush.
    /// </summary>
    public static readonly DependencyProperty NormalBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(NormalBorderBrush));

    /// <summary>
    /// The dependency property of hover border brush.
    /// </summary>
    public static readonly DependencyProperty HoverBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverBorderBrush));

    /// <summary>
    /// The dependency property of disabled border brush.
    /// </summary>
    public static readonly DependencyProperty DisabledBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DisabledBorderBrush));

    /// <summary>
    /// The dependency property of normal border thickness.
    /// </summary>
    public static readonly DependencyProperty NormalBorderThicknessProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(NormalBorderThickness));

    /// <summary>
    /// The dependency property of hover border thickness.
    /// </summary>
    public static readonly DependencyProperty HoverBorderThicknessProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(HoverBorderThickness));

    /// <summary>
    /// The dependency property of disabled border thickness.
    /// </summary>
    public static readonly DependencyProperty DisabledBorderThicknessProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(DisabledBorderThickness));

    /// <summary>
    /// The dependency property of text wrapping.
    /// </summary>
    public static readonly DependencyProperty TextWrappingProperty = DependencyObjectProxy.RegisterProperty<TextWrapping>(nameof(TextWrapping));

    /// <summary>
    /// The dependency property of text trimming.
    /// </summary>
    public static readonly DependencyProperty TextTrimmingProperty = DependencyObjectProxy.RegisterProperty<TextTrimming>(nameof(TextTrimming));
    
    /// <summary>
    /// The dependency property of image URI.
    /// </summary>
    public static readonly DependencyProperty ImageSourceProperty = DependencyObjectProxy.RegisterProperty<ImageSource>(nameof(ImageSource), OnImageUriChanged);

    /// <summary>
    /// The dependency property of image width.
    /// </summary>
    public static readonly DependencyProperty ImageWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(ImageWidth));

    /// <summary>
    /// The dependency property of image height.
    /// </summary>
    public static readonly DependencyProperty ImageHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(ImageHeight));

    /// <summary>
    /// The dependency property of image corner radius.
    /// </summary>
    public static readonly DependencyProperty ImageCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(ImageCornerRadius));

    /// <summary>
    /// The dependency property of image stretch.
    /// </summary>
    public static readonly DependencyProperty StretchProperty = DependencyObjectProxy.RegisterProperty<Stretch>(nameof(Stretch));

    /// <summary>
    /// The dependency property of icon URI.
    /// </summary>
    public static readonly DependencyProperty IconSourceProperty = DependencyObjectProxy.RegisterProperty<IconSource>(nameof(IconSource), OnIconUriChanged);

    /// <summary>
    /// The dependency property of icon width.
    /// </summary>
    public static readonly DependencyProperty IconWidthProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconWidth));

    /// <summary>
    /// The dependency property of icon height.
    /// </summary>
    public static readonly DependencyProperty IconHeightProperty = DependencyObjectProxy.RegisterDoubleProperty(nameof(IconHeight));

    /// <summary>
    /// Initializes a new instance of the TextButton class.
    /// </summary>
    public TextButton()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Occurs when this control is clicked.
    /// </summary>
    public event RoutedEventHandler Click;

    /// <summary>
    /// Occurs when the text trimmed property value has changed.
    /// </summary>
    public event TypedEventHandler<TextButton, IsTextTrimmedChangedEventArgs> IsTextTrimmedChanged;

    /// <summary>
    /// Occurs when there is an error associated with image retrieval or format.
    /// </summary>
    public event ExceptionRoutedEventHandler ImageFailed;

    /// <summary>
    /// Occurs when the image source is downloaded and decoded with no failure.
    /// </summary>
    public event RoutedEventHandler ImageOpened;

    /// <summary>
    /// Gets or sets a value that indicates when the click event occurs, in terms of device behavior.
    /// </summary>
    public ClickMode ClickMode
    {
        get => OwnerButton.ClickMode;
        set => OwnerButton.ClickMode = value;
    }

    /// <summary>
    /// Gets or sets the command to invoke when this button is pressed.
    /// </summary>
    public ICommand Command
    {
        get => OwnerButton.Command;
        set => OwnerButton.Command = value;
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the Command property.
    /// </summary>
    public object CommandParameter
    {
        get => OwnerButton.CommandParameter;
        set => OwnerButton.CommandParameter = value;
    }

    /// <summary>
    /// Gets a value that indicates whether a device pointer is located over this button control.
    /// </summary>
    public bool IsPointerOver => OwnerButton.IsPointerOver;

    /// <summary>
    /// Gets a value that indicates whether a ButtonBase is currently in a pressed state.
    /// </summary>
    public bool IsPressed => OwnerButton.IsPressed;

    /// <summary>
    /// Gets or sets the button template.
    /// </summary>
    public ControlTemplate ButtonTemplate
    {
        get => OwnerButton.Template;
        set => OwnerButton.Template = value;
    }

    /// <summary>
    /// Gets or sets the additional state.
    /// </summary>
    public string AdditionalState
    {
        get => (string)GetValue(AdditionalStateProperty);
        set => SetValue(AdditionalStateProperty, value);
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
    /// Gets the collection of inline text elements.
    /// </summary>
    public InlineCollection Inlines => TextControl.Inlines;

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
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the text wrapping mode.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    /// <summary>
    /// Gets or sets the text trimming mode.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    /// <summary>
    /// Gets or sets the space.
    /// </summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
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
    /// Gets or sets the disabled foreground.
    /// </summary>
    public Brush DisabledForeground
    {
        get => (Brush)GetValue(DisabledForegroundProperty);
        set => SetValue(DisabledForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the normal background.
    /// </summary>
    public Brush NormalBackground
    {
        get => (Brush)GetValue(NormalBackgroundProperty);
        set => SetValue(NormalBackgroundProperty, value);
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
    /// Gets or sets the disabled background.
    /// </summary>
    public Brush DisabledBackground
    {
        get => (Brush)GetValue(DisabledBackgroundProperty);
        set => SetValue(DisabledBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the normal border brush.
    /// </summary>
    public Brush NormalBorderBrush
    {
        get => (Brush)GetValue(NormalBorderBrushProperty);
        set => SetValue(NormalBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the hover border brush.
    /// </summary>
    public Brush HoverBorderBrush
    {
        get => (Brush)GetValue(HoverBorderBrushProperty);
        set => SetValue(HoverBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the disabled border brush.
    /// </summary>
    public Brush DisabledBorderBrush
    {
        get => (Brush)GetValue(DisabledBorderBrushProperty);
        set => SetValue(DisabledBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the normal border thickness.
    /// </summary>
    public Thickness NormalBorderThickness
    {
        get => (Thickness)GetValue(NormalBorderThicknessProperty);
        set => SetValue(NormalBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the hover border thickness.
    /// </summary>
    public Thickness HoverBorderThickness
    {
        get => (Thickness)GetValue(HoverBorderThicknessProperty);
        set => SetValue(HoverBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the disabled border thickness.
    /// </summary>
    public Thickness DisabledBorderThickness
    {
        get => (Thickness)GetValue(DisabledBorderThicknessProperty);
        set => SetValue(DisabledBorderThicknessProperty, value);
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
    /// Gets or sets the image source.
    /// </summary>
    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
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
    /// Gets or sets the stretch of image.
    /// </summary>
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
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
    /// Gets or sets the icon source.
    /// </summary>
    public IconSource IconSource
    {
        get => (IconSource)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the child before content.
    /// </summary>
    public UIElement BeforeContentChild
    {
        get => BeforeElement.Child;
        set => BeforeElement.Child = value;
    }

    /// <summary>
    /// Gets or sets the child after content.
    /// </summary>
    public UIElement AfterContentChild
    {
        get => AfterElement.Child;
        set => AfterElement.Child = value;
    }

    /// <summary>
    /// Gets or sets the child in normal state.
    /// </summary>
    public UIElement NormalChild
    {
        get => NormalPanel.Child;
        set => NormalPanel.Child = value;
    }

    /// <summary>
    /// Gets or sets the child in hover state.
    /// </summary>
    public UIElement HoverChild
    {
        get => HoverPanel.Child;
        set => HoverPanel.Child = value;
    }

    /// <summary>
    /// Registers a click event handler.
    /// </summary>
    /// <param name="click">The click event handler.</param>
    /// <param name="options">The interceptor policy.</param>
    /// <returns>The handler registered.</returns>
    public RoutedEventHandler RegisterClick(RoutedEventHandler click, InterceptorPolicy options = null)
    {
        if (click == null) return null;
        if (options == null)
        {
            Click += click;
            return click;
        }

        var action = Interceptor.Action<object, RoutedEventArgs>((sender, ev) => click(sender, ev), options);
        void h(object sender, RoutedEventArgs ev)
        {
            action(sender, ev);
        }
        
        Click += h;
        return h;
    }

    /// <summary>
    /// Transitions a control between two states, by requesting a new VisualState by name.
    /// </summary>
    /// <param name="name">The state to transition to.</param>
    /// <returns>true if the control successfully transitions to the new state, or was already using that state; otherwise, false.</returns>
    public bool GoToButtonState(string name)
        => VisualStateManager.GoToState(OwnerButton, name, true);

    /// <summary>
    /// Sets image.
    /// </summary>
    /// <param name="uri">The image URI.</param>
    /// <returns>The image source.</returns>
    public ImageSource SetImage(Uri uri)
        => ImageSource = new BitmapImage
        {
            UriSource = uri
        };

    private void Button_Click(object sender, RoutedEventArgs e)
        => Click?.Invoke(this, e);

    private static void OnSpacingChanged(TextButton c, ChangeEventArgs<double> e, DependencyProperty p)
    {
        var i = double.IsNaN(e.NewValue) ? 0 : e.NewValue;
        c.ImagePanel.Margin = c.IconControl.Margin = new Thickness(0, 0, i, 0);
    }

    private static void OnImageUriChanged(TextButton c, ChangeEventArgs<ImageSource> e, DependencyProperty p)
        => c.ImagePanel.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;

    private static void OnIconUriChanged(TextButton c, ChangeEventArgs<IconSource> e, DependencyProperty p)
        => c.IconControl.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;

    private void OwnerButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        NormalPanel.Visibility = Visibility.Collapsed;
        HoverPanel.Visibility = Visibility.Visible;
    }

    private void OwnerButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        NormalPanel.Visibility = Visibility.Visible;
        HoverPanel.Visibility = Visibility.Collapsed;
    }

    private void ImageControl_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        => ImageFailed?.Invoke(this, e);

    private void ImageControl_ImageOpened(object sender, RoutedEventArgs e)
        => ImageOpened?.Invoke(this, e);

    private void TextBlock_IsTextTrimmedChanged(TextBlock sender, IsTextTrimmedChangedEventArgs args)
        => IsTextTrimmedChanged?.Invoke(this, args);
}
