using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Trivial.Data;
using Trivial.Tasks;
using Trivial.Text;
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
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (AggregateException)
        {
        }
        catch (ApplicationException)
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
        => TryGetAppWindow(window)?.Presenter?.Kind ?? AppWindowPresenterKind.Default;

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
        var appWin = TryGetAppWindow(window);
        return SetFullScreenMode(kind, appWin);
    }

    /// <summary>
    /// Attempts to place the app in full-screen mode or others.
    /// </summary>
    /// <param name="fullScreen">true if place the app in full-screen mode; otherwise, false.</param>
    /// <param name="window">The window to set.</param>
    /// <returns>The presenter kind applied to the window.</returns>
    public static AppWindowPresenterKind SetFullScreenMode(bool fullScreen, AppWindow window = null)
        => SetFullScreenMode(fullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default, window);

    /// <summary>
    /// Attempts to place the app in full-screen mode or others.
    /// </summary>
    /// <param name="kind">The specified presenter kind to apply to the window.</param>
    /// <param name="window">The window to set.</param>
    /// <returns>The presenter kind applied to the window.</returns>
    public static AppWindowPresenterKind SetFullScreenMode(AppWindowPresenterKind kind, AppWindow window = null)
    {
        try
        {
            if (window == null) return AppWindowPresenterKind.Default;
            window.SetPresenter(kind);
            return window.Presenter.Kind;
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ExternalException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (IOException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (AggregateException)
        {
        }
        catch (ApplicationException)
        {
        }

        return AppWindowPresenterKind.Default;
    }

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
    /// Gets app window instance of given window.
    /// </summary>
    /// <param name="window">The window object.</param>
    /// <returns>The app window.</returns>
    public static AppWindow TryGetAppWindow(Window window)
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
        catch (ExternalException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (IOException)
        {
        }
        catch (SecurityException)
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
        catch (ApplicationException)
        {
        }
        catch (InvalidCastException)
        {
        }

        return null;
    }

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The solid brush.</returns>
    public static SolidColorBrush ToBrush(Windows.UI.Color color)
        => new(color);

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The solid brush.</returns>
    public static SolidColorBrush ToBrush(System.Drawing.Color color)
        => new(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));

    /// <summary>
    /// Create text inlines.
    /// </summary>
    /// <param name="json">The data source.</param>
    /// <param name="style">The style.</param>
    /// <returns>The inline collection</returns>
    public static IEnumerable<Inline> CreateTextInlines(JsonObjectNode json, JsonTextStyle style)
    {
        var arr = new List<Inline>();
        if (json == null) return arr;
        CreateTextInlines(arr, json, style ?? new(), 0);
        return arr;
    }

    /// <summary>
    /// Create text inlines.
    /// </summary>
    /// <param name="inlines">The inline collection.</param>
    /// <param name="json">The data source.</param>
    /// <param name="style">The style.</param>
    /// <returns>The inline collection</returns>
    public static IEnumerable<Inline> CreateTextInlines(InlineCollection inlines, JsonObjectNode json, JsonTextStyle style)
    {
        var col = CreateTextInlines(json, style);
        foreach (var l in col)
        {
            inlines.Add(l);
        }

        return col;
    }

    /// <summary>
    /// Create text inlines.
    /// </summary>
    /// <param name="json">The data source.</param>
    /// <param name="style">The style.</param>
    /// <returns>The inline collection</returns>
    public static IEnumerable<Inline> CreateTextInlines(JsonArrayNode json, JsonTextStyle style)
    {
        var arr = new List<Inline>();
        if (json == null) return arr;
        CreateTextInlines(arr, json, style ?? new(), 0);
        return arr;
    }

    /// <summary>
    /// Create text inlines.
    /// </summary>
    /// <param name="inlines">The inline collection.</param>
    /// <param name="json">The data source.</param>
    /// <param name="style">The style.</param>
    /// <returns>The inline collection</returns>
    public static IEnumerable<Inline> CreateTextInlines(InlineCollection inlines, JsonArrayNode json, JsonTextStyle style)
    {
        var col = CreateTextInlines(json, style);
        foreach (var l in col)
        {
            inlines.Add(l);
        }

        return col;
    }

    private static void CreateTextInlines(List<Inline> arr, JsonObjectNode json, JsonTextStyle style, int intend)
    {
        if (json == null) return;
        var blank = new string(' ', intend * 4);
        arr.Add(CreateRun('{', style.PunctuationForeground));
        var i = 0;
        var blank2 = string.Concat(blank, "    ");
        foreach (var prop in json)
        {
            if (prop.Value is null) continue;
            if (i > 0) arr.Add(CreateRun(',', style.PunctuationForeground));
            arr.Add(new LineBreak());
            arr.Add(CreateRun(blank2, null));
            arr.Add(CreateRun(JsonStringNode.ToJson(prop.Key), style.PropertyForeground));
            arr.Add(CreateRun(": ", style.PunctuationForeground));
            CreateTextInlines(arr, prop.Value, style, intend + 1);
            i++;
        }

        arr.Add(new LineBreak());
        arr.Add(CreateRun(blank, null));
        arr.Add(CreateRun('}', style.PunctuationForeground));
    }

    private static void CreateTextInlines(List<Inline> arr, JsonArrayNode json, JsonTextStyle style, int intend)
    {
        if (json == null) return;
        var blank = new string(' ', intend * 4);
        arr.Add(CreateRun('[', style.PunctuationForeground));
        var i = 0;
        var blank2 = string.Concat(blank, "    ");
        foreach (var item in json)
        {
            if (item is null) continue;
            if (i > 0) arr.Add(CreateRun(',', style.PunctuationForeground));
            arr.Add(new LineBreak());
            arr.Add(CreateRun(blank2, null));
            CreateTextInlines(arr, item, style, intend + 1);
            i++;
        }

        arr.Add(new LineBreak());
        arr.Add(CreateRun(blank, null));
        arr.Add(CreateRun(']', style.PunctuationForeground));
    }

    private static void CreateTextInlines(List<Inline> arr, IJsonDataNode json, JsonTextStyle style, int intend)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                arr.Add(CreateRun("null", style.KeywordForeground));
                break;
            case JsonValueKind.String:
                arr.Add(CreateRun(json.ToString(), style.StringForeground));
                break;
            case JsonValueKind.Number:
                arr.Add(CreateRun(json.ToString(), style.NumberForeground));
                break;
            case JsonValueKind.True:
                arr.Add(CreateRun("true", style.KeywordForeground));
                break;
            case JsonValueKind.False:
                arr.Add(CreateRun("false", style.KeywordForeground));
                break;
            case JsonValueKind.Object:
                CreateTextInlines(arr, json as JsonObjectNode, style, intend);
                break;
            case JsonValueKind.Array:
                CreateTextInlines(arr, json as JsonArrayNode, style, intend);
                break;
            default:
                break;
        }
    }

    private static Run CreateRun(string text, Brush foreground)
        => new()
        {
            Foreground = foreground,
            Text = text
        };

    private static Run CreateRun(char text, Brush foreground)
        => new()
        {
            Foreground = foreground,
            Text = text.ToString()
        };
}
