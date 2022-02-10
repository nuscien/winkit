using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// <exception cref="COMException">COM exception.</exception>
    public static T GetResource<T>(string key)
    {
        return (T)Application.Current.Resources[key];
    }

    /// <summary>
    /// Tries to get the resource.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The resource.</returns>
    public static T TryGetResource<T>(string key, T defaultValue = default)
        => TryGetResource<T>(key, out var r) ? r : defaultValue;

    /// <summary>
    /// Tries to get the resource.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <param name="result">The result output.</param>
    /// <returns>true if get succeeded; otherwise, false.</returns>
    /// <exception cref="InvalidCastException">The type is not the expected one.</exception>
    /// <exception cref="ArgumentException">key is invalid.</exception>
    /// <exception cref="COMException">COM exception.</exception>
    public static bool TryGetResource<T>(string key, out T result)
    {
        if (string.IsNullOrEmpty(key))
        {
            result = default;
            return false;
        }

        try
        {
            var v = Application.Current.Resources[key];
            if (v is T r)
            {
                result = r;
                return true;
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (AggregateException)
        {
        }

        result = default;
        return false;
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
