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
    public static readonly DependencyProperty SpacingProperty = DependencyObjectProxy.RegisterProperty(nameof(Spacing), 0d);

    /// <summary>
    /// The dependency property of hover foreground.
    /// </summary>
    public static readonly DependencyProperty HoverForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverForeground));

    /// <summary>
    /// The dependency property of pressed foreground.
    /// </summary>
    public static readonly DependencyProperty PressedForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(PressedForeground));

    /// <summary>
    /// The dependency property of disabled foreground.
    /// </summary>
    public static readonly DependencyProperty DisabledForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DisabledForeground));

    /// <summary>
    /// The dependency property of base background.
    /// </summary>
    public static readonly DependencyProperty BaseBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(BaseBackground));

    /// <summary>
    /// The dependency property of hover background.
    /// </summary>
    public static readonly DependencyProperty HoverBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverBackground));

    /// <summary>
    /// The dependency property of pressed background.
    /// </summary>
    public static readonly DependencyProperty PressedBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(PressedBackground));

    /// <summary>
    /// The dependency property of pressed background.
    /// </summary>
    public static readonly DependencyProperty DisabledBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DisabledBackground));

    /// <summary>
    /// The dependency property of hover cover background.
    /// </summary>
    public static readonly DependencyProperty HoverCoverBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverCoverBackground));

    /// <summary>
    /// The dependency property of hover border brush.
    /// </summary>
    public static readonly DependencyProperty HoverBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(HoverBorderBrush));

    /// <summary>
    /// The dependency property of pressed border brush.
    /// </summary>
    public static readonly DependencyProperty PressedBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(PressedBorderBrush));

    /// <summary>
    /// The dependency property of pressed border brush.
    /// </summary>
    public static readonly DependencyProperty DisabledBorderBrushProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(DisabledBorderBrush));

    /// <summary>
    /// The dependency property of hover border thickness.
    /// </summary>
    public static readonly DependencyProperty HoverBorderThicknessProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(HoverBorderThickness));

    /// <summary>
    /// The dependency property of pressed border thickness.
    /// </summary>
    public static readonly DependencyProperty PressedBorderThicknessProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(PressedBorderThickness));

    /// <summary>
    /// The dependency property of pressed border thickness.
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
    public static readonly DependencyProperty ImageStretchProperty = DependencyObjectProxy.RegisterProperty<Stretch>(nameof(Stretch));

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
    /// Gets or sets the text.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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
    /// Gets or sets the pressed foreground.
    /// </summary>
    public Brush PressedForeground
    {
        get => (Brush)GetValue(PressedForegroundProperty);
        set => SetValue(PressedForegroundProperty, value);
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
    /// Gets or sets the hover cover background.
    /// </summary>
    public Brush BaseBackground
    {
        get => (Brush)GetValue(BaseBackgroundProperty);
        set => SetValue(BaseBackgroundProperty, value);
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
    /// Gets or sets the disabled background.
    /// </summary>
    public Brush DisabledBackground
    {
        get => (Brush)GetValue(DisabledBackgroundProperty);
        set => SetValue(DisabledBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the hover cover background.
    /// </summary>
    public Brush HoverCoverBackground
    {
        get => (Brush)GetValue(HoverCoverBackgroundProperty);
        set => SetValue(HoverCoverBackgroundProperty, value);
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
    /// Gets or sets the pressed border brush.
    /// </summary>
    public Brush PressedBorderBrush
    {
        get => (Brush)GetValue(PressedBorderBrushProperty);
        set => SetValue(PressedBorderBrushProperty, value);
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
    /// Gets or sets the hover border thickness.
    /// </summary>
    public Thickness HoverBorderThickness
    {
        get => (Thickness)GetValue(HoverBorderThicknessProperty);
        set => SetValue(HoverBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the pressed border thickness.
    /// </summary>
    public Thickness PressedBorderThickness
    {
        get => (Thickness)GetValue(PressedBorderThicknessProperty);
        set => SetValue(PressedBorderThicknessProperty, value);
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
        get => (Stretch)GetValue(ImageStretchProperty);
        set => SetValue(ImageStretchProperty, value);
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

    private void Button_Click(object sender, RoutedEventArgs e)
        => Click?.Invoke(this, e);

    private static void OnImageUriChanged(TextButton c, ChangeEventArgs<ImageSource> e, DependencyProperty p)
    {
        c.ImagePanel.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;
    }
}
