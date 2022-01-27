using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Trivial.Data;
using Trivial.Tasks;
using Windows.UI.Text;

namespace Trivial.UI;

/// <summary>
/// The static proxy for dependency object.
/// </summary>
public static class DependencyObjectProxy<TControl> where TControl : DependencyObject
{
    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <typeparam name="TPropertyValue">The type of property value.</typeparam>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <param name="converter">The property value converter.</param>
    /// <param name="stillUpdateEvenIfSame">true if still raise the event handler even on same; otherwise, false.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterProperty<TPropertyValue>(string name, Action<TControl, ChangeEventArgs<TPropertyValue>, DependencyProperty> callback, TPropertyValue defaultValue = default, Func<object, TPropertyValue> converter = null, bool stillUpdateEvenIfSame = false)
        => DependencyProperty.Register(name, typeof(TPropertyValue), typeof(TControl), new PropertyMetadata(defaultValue, callback == null ? null : (d, e) =>
        {
            var c = d as TControl;
            var changeMethod = ChangeMethods.Update;
            if (converter != null)
            {
                var newValue = converter(e.NewValue);
                var oldValue = converter(e.OldValue);
                OnChange(c, name, callback, newValue, oldValue, changeMethod, stillUpdateEvenIfSame, e.Property);
            }
            else
            {
                if (e.NewValue is not TPropertyValue newValue)
                {
                    if (e.NewValue is not null) return;
                    newValue = defaultValue;
                    changeMethod = ChangeMethods.Remove;
                }

                if (e.OldValue is not TPropertyValue oldValue)
                {
                    if (e.OldValue is not null) return;
                    oldValue = defaultValue;
                    changeMethod = ChangeMethods.Add;
                }

                OnChange(c, name, callback, newValue, oldValue, changeMethod, stillUpdateEvenIfSame, e.Property);
            }
        }));

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <typeparam name="TPropertyValue">The type of property value.</typeparam>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterProperty<TPropertyValue>(string name, PropertyChangedCallback callback, TPropertyValue defaultValue = default)
        => DependencyProperty.Register(name, typeof(TPropertyValue), typeof(TControl), new PropertyMetadata(defaultValue, callback));

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <typeparam name="TPropertyValue">The type of property value.</typeparam>
    /// <param name="name">The property name.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterProperty<TPropertyValue>(string name, TPropertyValue defaultValue = default)
        => DependencyProperty.Register(name, typeof(TPropertyValue), typeof(TControl), new PropertyMetadata(defaultValue));

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <param name="stillUpdateEvenIfSame">true if still raise the event handler even on same; otherwise, false.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterInt32Property(string name, Action<TControl, ChangeEventArgs<int>, DependencyProperty> callback, int defaultValue = 0, bool stillUpdateEvenIfSame = false)
        => RegisterProperty(name, callback, defaultValue, v => {
            if (v is null) return defaultValue;
            if (v is int i) return i;
            if (v is long l) return (int)l;
            if (v is float f) return (int)Math.Round(f);
            if (v is double d) return (int)Math.Round(d);
            if (v is decimal d2) return (int)Math.Round(d2);
            if (v is string s) return Maths.Numbers.ParseToInt32(s, 10);
            return (int)v;
        }, stillUpdateEvenIfSame);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <param name="stillUpdateEvenIfSame">true if still raise the event handler even on same; otherwise, false.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterInt64Property(string name, Action<TControl, ChangeEventArgs<long>, DependencyProperty> callback, long defaultValue = 0L, bool stillUpdateEvenIfSame = false)
        => RegisterProperty(name, callback, defaultValue, v => {
            if (v is null) return defaultValue;
            if (v is long l) return l;
            if (v is int i) return i;
            if (v is float f) return (long)Math.Round(f);
            if (v is double d) return (long)Math.Round(d);
            if (v is decimal d2) return (long)Math.Round(d2);
            if (v is string s) return Maths.Numbers.ParseToInt64(s, 10);
            return (long)v;
        }, stillUpdateEvenIfSame);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterSingleProperty(string name, Action<TControl, ChangeEventArgs<float>, DependencyProperty> callback = null, float defaultValue = float.NaN)
        => RegisterProperty(name, callback, defaultValue);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterDoubleProperty(string name, Action<TControl, ChangeEventArgs<double>, DependencyProperty> callback = null, double defaultValue = double.NaN)
        => RegisterProperty(name, callback, defaultValue);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterFontWeightProperty(string name, Action<TControl, ChangeEventArgs<FontWeight>, DependencyProperty> callback = null, FontWeight? defaultValue = null)
        => RegisterProperty(name, callback, defaultValue ?? FontWeights.Normal);

    /// <summary>
    /// Occurs on animated button pointer entered.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    public static void AnimatedButtonPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button || button.Content is not AnimatedIcon icon) return;
        AnimatedIcon.SetState(icon, "PointerOver");
    }

    /// <summary>
    /// Occurs on animated button pointer exitesd.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    public static void AnimatedButtonPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button || button.Content is not AnimatedIcon icon) return;
        AnimatedIcon.SetState(icon, "PointerOver");
    }

    /// <summary>
    /// Finds a visual child of the control type.
    /// </summary>
    /// <param name="obj">The parent control.</param>
    /// <returns>The first child found.</returns>
    public static TControl FindVisualChild(DependencyObject obj)
        => FindVisualChild(obj, short.MaxValue);

    /// <summary>
    /// Finds a visual child of the control type.
    /// </summary>
    /// <param name="obj">The parent control.</param>
    /// <param name="deepLevel">The child levels to find recurrence.</param>
    /// <returns>The first child found.</returns>
    public static TControl FindVisualChild(DependencyObject obj, int deepLevel)
    {
        if (obj == null || deepLevel <= 0) return null;
        deepLevel--;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child != null && child is TControl c)
                return c;
            c = FindVisualChild(child, deepLevel);
            if (c != null)
                return c;
        }

        return null;
    }

    private static void OnChange<TPropertyValue>(TControl c, string name, Action<TControl, ChangeEventArgs<TPropertyValue>, DependencyProperty> callback, TPropertyValue newValue, TPropertyValue oldValue, ChangeMethods changeMethod, bool stillUpdateEvenIfSame, DependencyProperty p)
    {
        if (newValue is null)
        {
            if (oldValue is null)
            {
                if (stillUpdateEvenIfSame) changeMethod = ChangeMethods.Same;
                else return;
            }
            else
            {
                changeMethod = ChangeMethods.Remove;
            }
        }
        else
        {
            if (oldValue is null) changeMethod = ChangeMethods.Add;
            if (newValue.Equals(oldValue))
            {
                if (stillUpdateEvenIfSame) changeMethod = ChangeMethods.Same;
                else return;
            }
        }

        callback(c, new ChangeEventArgs<TPropertyValue>(oldValue, newValue, changeMethod, name), p);
    }
}

/// <summary>
/// The utilities of visual element.
/// </summary>
public static class VisualUtilities
{
    /// <summary>
    /// Gets the resource.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <returns>The resource.</returns>
    /// <exception cref="InvalidCastException">The type is not the expected one.</exception>
    /// <exception cref="ArgumentException">key is invalid.</exception>
    public static T GetResource<T>(string key)
    {
        return (T)Application.Current.Resources[key];
    }

    /// <summary>
    /// Registers a click event handler.
    /// </summary>
    /// <param name="button">The button control.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="options">The interceptor policy.</param>
    /// <returns>The handler registered.</returns>
    public static RoutedEventHandler RegisterClick(Microsoft.UI.Xaml.Controls.Primitives.ButtonBase button, RoutedEventHandler click, InterceptorPolicy options = null)
    {
        if (click == null) return null;
        if (options == null)
        {
            button.Click += click;
            return click;
        }

        var action = Interceptor.Action<object, RoutedEventArgs>((sender, ev) => click(sender, ev), options);
        void h(object sender, RoutedEventArgs ev)
        {
            action(sender, ev);
        }

        button.Click += h;
        return h;
    }

    /// <summary>
    /// Gets if the app in full-screen mode.
    /// </summary>
    /// <param name="window">The window to get.</param>
    /// <returns>The presenter kind applied to the window.</returns>
    public static AppWindowPresenterKind GetFullScreenMode(Window window = null)
        => GetAppWindow(window)?.Presenter?.Kind ?? AppWindowPresenterKind.Default;

    /// <summary>
    /// Attempts to place the app in full-screen mode or others.
    /// </summary>
    /// <param name="fullScreen">true if place the app in full-screen mode; otherwise, false.</param>
    /// <param name="window">The window to set.</param>
    /// <returns>The presenter kind applied to the window.</returns>
    public static AppWindowPresenterKind SetFullScreenMode(bool fullScreen, Window window = null)
        => SetFullScreenMode(fullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default, window);

    /// <summary>
    /// Attempts to place the app in full-screen mode or others.
    /// </summary>
    /// <param name="kind">The specified presenter kind to apply to the window.</param>
    /// <param name="window">The window to set.</param>
    /// <returns>The presenter kind applied to the window.</returns>
    public static AppWindowPresenterKind SetFullScreenMode(AppWindowPresenterKind kind, Window window = null)
    {
        try
        {
            var appWin = GetAppWindow(window);
            if (appWin == null) return AppWindowPresenterKind.Default;
            appWin.SetPresenter(kind);
            return appWin.Presenter.Kind;
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (System.IO.IOException)
        {
        }
        catch (System.Security.SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return AppWindowPresenterKind.Default;
    }

    private static AppWindow GetAppWindow(Window window)
    {
        try
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window ?? Window.Current);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            return AppWindow.GetFromWindowId(windowId);
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (System.IO.IOException)
        {
        }
        catch (System.Security.SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (AggregateException)
        {
        }
        catch (ArgumentException)
        {
        }

        return null;
    }
}
