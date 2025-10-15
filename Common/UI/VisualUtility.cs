using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trivial.Data;
using Trivial.Maths;
using Trivial.Tasks;
using Trivial.Text;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.ViewManagement;

namespace Trivial.UI;

/// <summary>
/// The utilities of visual element.
/// </summary>
public static partial class VisualUtility
{
    internal readonly static Brush TransparentBrush = new SolidColorBrush();

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
        => (T)Application.Current.Resources[key];

    /// <summary>
    /// Tries to get the resource.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The resource.</returns>
    public static T TryGetResource<T>(string key, T defaultValue = default)
        => TryGetResource<T>(Application.Current.Resources, key, out var r) ? r : defaultValue;

    /// <summary>
    /// Tries to get the resource.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="resources">The resources.</param>
    /// <param name="key">The resource key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The resource.</returns>
    public static T TryGetResource<T>(ResourceDictionary resources, string key, T defaultValue = default)
        => TryGetResource<T>(resources, key, out var r) ? r : defaultValue;

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
        => TryGetResource(Application.Current.Resources, key, out result);

    /// <summary>
    /// Tries to get the resource.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="resources">The resources.</param>
    /// <param name="key">The resource key.</param>
    /// <param name="result">The result output.</param>
    /// <returns>true if get succeeded; otherwise, false.</returns>
    /// <exception cref="InvalidCastException">The type is not the expected one.</exception>
    /// <exception cref="ArgumentException">key is invalid.</exception>
    /// <exception cref="COMException">COM exception.</exception>
    public static bool TryGetResource<T>(ResourceDictionary resources, string key, out T result)
    {
        if (string.IsNullOrEmpty(key))
        {
            result = default;
            return false;
        }

        try
        {
            var v = (resources ?? Application.Current.Resources)[key];
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
    /// Tries to get the style.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <returns>The style; or null, if does not exist.</returns>
    public static Style TryGetStyle(string key)
        => TryGetResource<Style>(Application.Current.Resources, key, out var r) ? r : null;

    /// <summary>
    /// Tries to get the style.
    /// </summary>
    /// <param name="resources">The resources.</param>
    /// <param name="key">The resource key.</param>
    /// <returns>The style; or null, if does not exist.</returns>
    public static Style TryGetStyle(ResourceDictionary resources, string key)
        => TryGetResource<Style>(resources, key, out var r) ? r : null;

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
    public static void AnimatedButtonPointerOver(object sender, PointerRoutedEventArgs e)
    {
        if (sender is AnimatedIcon ai)
        {
            AnimatedIcon.SetState(ai, "PointerOver");
            return;
        }

        if (sender is not ContentControl button || button.Content is not AnimatedIcon icon) return;
        AnimatedIcon.SetState(icon, "PointerOver");
    }

    /// <summary>
    /// Occurs on animated button pointer exitesd.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    public static void AnimatedButtonNormal(object sender, PointerRoutedEventArgs e)
    {
        if (sender is AnimatedIcon ai)
        {
            AnimatedIcon.SetState(ai, "Normal");
            return;
        }

        if (sender is not ContentControl button || button.Content is not AnimatedIcon icon) return;
        AnimatedIcon.SetState(icon, "Normal");
    }

    /// <summary>
    /// Occurs on animated button pointer exitesd.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    public static void AnimatedButtonPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is AnimatedIcon ai)
        {
            AnimatedIcon.SetState(ai, "Pressed");
            return;
        }

        if (sender is not ContentControl button || button.Content is not AnimatedIcon icon) return;
        AnimatedIcon.SetState(icon, "Pressed");
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
            var w = window.AppWindow;
            if (w is not null) return w;
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
    public static SolidColorBrush ToBrush(Color color)
        => new(color);

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The solid brush.</returns>
    public static SolidColorBrush ToBrush(System.Drawing.Color color)
        => new(Color.FromArgb(color.A, color.R, color.G, color.B));

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="a">The value of alpha channel (0-255).</param>
    /// <param name="r">The value of red channel (0-255).</param>
    /// <param name="g">The value of green channel (0-255).</param>
    /// <param name="b">The value of blue channel (0-255).</param>
    /// <returns>The solid brush.</returns>
    public static SolidColorBrush ToBrush(byte a, byte r, byte g, byte b)
        => new(Color.FromArgb(a, r, g, b));

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="r">The value of red channel (0-255).</param>
    /// <param name="g">The value of green channel (0-255).</param>
    /// <param name="b">The value of blue channel (0-255).</param>
    /// <returns>The solid brush.</returns>
    public static SolidColorBrush ToBrush(byte r, byte g, byte b)
        => new(Color.FromArgb(255, r, g, b));

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="colorType">The UI color type. e.g. accent color.</param>
    /// <returns>The solid brush.</returns>
    public static SolidColorBrush ToBrush(UIColorType colorType)
        => new(ToColor(colorType));

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The color.</returns>
    public static Color ToColor(System.Drawing.Color color)
        => Color.FromArgb(color.A, color.R, color.G, color.B);

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="argb">The ARGB value.</param>
    /// <returns>The color.</returns>
    public static Color ToColor(int argb)
        => ToColor(System.Drawing.Color.FromArgb(argb));

    /// <summary>
    /// Converts to brush.
    /// </summary>
    /// <param name="colorType">The UI color type. e.g. accent color.</param>
    /// <returns>The color.</returns>
    public static Color ToColor(UIColorType colorType)
        => new UISettings().GetColorValue(colorType);

    /// <summary>
    /// Create text inlines.
    /// </summary>
    /// <param name="json">The data source.</param>
    /// <param name="style">The optional style for JSON.</param>
    /// <returns>The inline collection</returns>
    public static IList<Inline> CreateTextInlines(JsonObjectNode json, JsonTextStyle style = null)
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
    /// <param name="style">The optional style for JSON.</param>
    /// <param name="watcher">The optional watcher for each inline.</param>
    /// <returns>The inline collection</returns>
    public static IList<Inline> CreateTextInlines(InlineCollection inlines, JsonObjectNode json, JsonTextStyle style = null, Action<Inline> watcher = null)
    {
        var col = CreateTextInlines(json, style);
        if (inlines == null) return col;
        if (watcher == null)
        {
            foreach (var l in col)
            {
                inlines.Add(l);
            }
        }
        else
        {
            foreach (var l in col)
            {
                watcher(l);
                inlines.Add(l);
            }
        }

        return col;
    }

    /// <summary>
    /// Create text inlines.
    /// </summary>
    /// <param name="inlines">The inline collection.</param>
    /// <param name="json">The data source.</param>
    /// <param name="theme">The application theme.</param>
    /// <param name="isCompact">true if compact the white spaces; otherwise, false.</param>
    /// <param name="watcher">The optional watcher for each inline.</param>
    /// <returns>The inline collection</returns>
    public static IList<Inline> CreateTextInlines(InlineCollection inlines, JsonObjectNode json, ApplicationTheme theme, bool isCompact = false, Action<Inline> watcher = null)
        => CreateTextInlines(inlines, json, new JsonTextStyle(theme, isCompact), watcher);

    /// <summary>
    /// Create text inlines.
    /// </summary>
    /// <param name="json">The data source.</param>
    /// <param name="style">The optional style for JSON.</param>
    /// <returns>The inline collection</returns>
    public static IList<Inline> CreateTextInlines(JsonArrayNode json, JsonTextStyle style = null)
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
    /// <param name="style">The optional style for JSON.</param>
    /// <param name="watcher">The optional watcher for each inline.</param>
    /// <returns>The inline collection</returns>
    public static IList<Inline> CreateTextInlines(InlineCollection inlines, JsonArrayNode json, JsonTextStyle style = null, Action<Inline> watcher = null)
    {
        var col = CreateTextInlines(json, style);
        if (inlines == null) return col;
        if (watcher == null)
        {
            foreach (var l in col)
            {
                inlines.Add(l);
            }
        }
        else
        {
            foreach (var l in col)
            {
                watcher(l);
                inlines.Add(l);
            }
        }

        return col;
    }

    /// <summary>
    /// Create text inlines.
    /// </summary>
    /// <param name="inlines">The inline collection.</param>
    /// <param name="json">The data source.</param>
    /// <param name="theme">The application theme.</param>
    /// <param name="isCompact">true if compact the white spaces; otherwise, false.</param>
    /// <param name="watcher">The optional watcher for each inline.</param>
    /// <returns>The inline collection</returns>
    public static IList<Inline> CreateTextInlines(InlineCollection inlines, JsonArrayNode json, ApplicationTheme theme, bool isCompact = false, Action<Inline> watcher = null)
        => CreateTextInlines(inlines, json, new JsonTextStyle(theme, isCompact), watcher);

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="text">The content text.</param>
    /// <returns>The paragraph.</returns>
    public static Paragraph CreateTextParagraph(string text)
    {
        var inline = new Run
        {
            Text = text
        };
        var block = new Paragraph();
        block.Inlines.Add(inline);
        return block;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="text">The content text.</param>
    /// <param name="inlines">The additional inline collection.</param>
    /// <returns>The paragraph.</returns>
    public static Paragraph CreateTextParagraph(Inline text, params Inline[] inlines)
    {
        var block = new Paragraph();
        block.Inlines.Add(text);
        foreach (var inline in inlines)
        {
            block.Inlines.Add(inline);
        }

        return block;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="inlines">The inline collection.</param>
    /// <returns>The paragraph.</returns>
    public static Paragraph CreateTextParagraph(IEnumerable<Inline> inlines)
    {
        if (inlines == null) return null;
        var block = new Paragraph();
        foreach (var inline in inlines)
        {
            block.Inlines.Add(inline);
        }

        return block;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="blocks">The block collection to insert the paragraph at bottom.</param>
    /// <param name="text">The content text.</param>
    /// <returns>The paragraph.</returns>
    public static Paragraph CreateTextParagraph(BlockCollection blocks, string text)
    {
        var c = CreateTextParagraph(text);
        blocks?.Add(c);
        return c;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="blocks">The block collection to insert the paragraph at bottom.</param>
    /// <param name="text">The content text.</param>
    /// <param name="inlines">The additional inline collection.</param>
    /// <returns>The paragraph.</returns>
    public static Paragraph CreateTextParagraph(BlockCollection blocks, Inline text, params Inline[] inlines)
    {
        var c = CreateTextParagraph(text, inlines);
        blocks?.Add(c);
        return c;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="blocks">The block collection to insert the paragraph at bottom.</param>
    /// <param name="inlines">The inline collection.</param>
    /// <returns>The paragraph.</returns>
    public static Paragraph CreateTextParagraph(BlockCollection blocks, IEnumerable<Inline> inlines)
    {
        var c = CreateTextParagraph(inlines);
        blocks?.Add(c);
        return c;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="json">The data source.</param>
    /// <param name="style">The optional style for JSON.</param>
    /// <param name="watcher">The optional watcher for each inline.</param>
    /// <returns>The inline collection</returns>
    public static Paragraph CreateTextParagraph(JsonObjectNode json, JsonTextStyle style = null, Action<Inline> watcher = null)
    {
        var block = new Paragraph();
        CreateTextInlines(block.Inlines, json, style, watcher);
        return block;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="blocks">The block collection to insert the paragraph at bottom.</param>
    /// <param name="json">The data source.</param>
    /// <param name="style">The optional style for JSON.</param>
    /// <param name="watcher">The optional watcher for each inline.</param>
    /// <returns>The inline collection</returns>
    public static Paragraph CreateTextParagraph(BlockCollection blocks, JsonObjectNode json, JsonTextStyle style = null, Action<Inline> watcher = null)
    {
        var block = new Paragraph();
        CreateTextInlines(block.Inlines, json, style, watcher);
        blocks?.Add(block);
        return block;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="json">The data source.</param>
    /// <param name="style">The optional style for JSON.</param>
    /// <param name="watcher">The optional watcher for each inline.</param>
    /// <returns>The inline collection</returns>
    public static Paragraph CreateTextParagraph(JsonArrayNode json, JsonTextStyle style = null, Action<Inline> watcher = null)
    {
        var block = new Paragraph();
        CreateTextInlines(block.Inlines, json, style, watcher);
        return block;
    }

    /// <summary>
    /// Creates a paragraph.
    /// </summary>
    /// <param name="blocks">The block collection to insert the paragraph at bottom.</param>
    /// <param name="json">The data source.</param>
    /// <param name="style">The optional style for JSON.</param>
    /// <param name="watcher">The optional watcher for each inline.</param>
    /// <returns>The inline collection</returns>
    public static Paragraph CreateTextParagraph(BlockCollection blocks, JsonArrayNode json, JsonTextStyle style = null, Action<Inline> watcher = null)
    {
        var block = new Paragraph();
        CreateTextInlines(block.Inlines, json, style, watcher);
        blocks?.Add(block);
        return block;
    }

    /// <summary>
    /// Creates a button.
    /// </summary>
    /// <param name="content">The button content.</param>
    /// <param name="click">The click event.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>A button created.</returns>
    public static Button CreateButton(object content, RoutedEventHandler click, Style style = null)
    {
        var c = new Button
        {
            Content = content
        };
        if (style != null) c.Style = style;
        c.Click += click;
        return c;
    }

    /// <summary>
    /// Creates a button.
    /// </summary>
    /// <param name="content">The button content.</param>
    /// <param name="click">The click event.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>A button created.</returns>
    public static Button CreateButton(object content, Action click, Style style = null)
        => CreateButton(content, click != null ? (sender, ev) => click() : null, style);

    /// <summary>
    /// Creates a menu flyout item.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="icon">The graphic content.</param>
    /// <param name="click">The click event.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>A button created.</returns>
    public static MenuFlyoutItem CreateMenuFlyoutItem(string text, IconElement icon, RoutedEventHandler click, Style style = null)
    {
        var c = new MenuFlyoutItem
        {
            Text = text
        };
        if (icon != null) c.Icon = icon;
        if (style != null) c.Style = style;
        c.Click += click;
        return c;
    }

    /// <summary>
    /// Creates a menu flyout item.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="glyph">The character code of the icon content.</param>
    /// <param name="click">The click event.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>A button created.</returns>
    public static MenuFlyoutItem CreateMenuFlyoutItem(string text, char glyph, RoutedEventHandler click, Style style = null)
    {
        var c = new FontIcon
        {
            Glyph = glyph.ToString()
        };
        return CreateMenuFlyoutItem(text, c, click, style);
    }

    /// <summary>
    /// Creates a menu flyout item.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="symbol">The symbol.</param>
    /// <param name="click">The click event.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>A button created.</returns>
    public static MenuFlyoutItem CreateMenuFlyoutItem(string text, Symbol symbol, RoutedEventHandler click, Style style = null)
    {
        var c = new SymbolIcon
        {
            Symbol = symbol
        };
        return CreateMenuFlyoutItem(text, c, click, style);
    }

    /// <summary>
    /// Creates a menu flyout item.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="icon">The URI of the icon content.</param>
    /// <param name="click">The click event.</param>
    /// <param name="showAsMonochrome">true if need to show the bitmap in a single color; otherwise, show the bitmap in full color.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>A button created.</returns>
    public static MenuFlyoutItem CreateMenuFlyoutItem(string text, Uri icon, RoutedEventHandler click, bool showAsMonochrome = true, Style style = null)
    {
        var c = new BitmapIcon
        {
            UriSource = icon,
            ShowAsMonochrome = showAsMonochrome
        };
        return CreateMenuFlyoutItem(text, c, click, style);
    }

    /// <summary>
    /// Creates a menu flyout item.
    /// </summary>
    /// <param name="menu">The menu to insert the button at bottom.</param>
    /// <param name="text">The text content.</param>
    /// <param name="icon">The graphic content.</param>
    /// <param name="click">The click event.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>A button created.</returns>
    public static MenuFlyoutItem CreateMenuFlyoutItem(MenuFlyout menu, string text, IconElement icon, RoutedEventHandler click, Style style = null)
    {
        var c = CreateMenuFlyoutItem(text, icon, click, style);
        menu?.Items.Add(c);
        return c;
    }

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(string name, IconElement icon, RoutedEventHandler click, Style style = null)
    {
        var button = new AppBarButton
        {
            Label = name,
            Icon = icon
        };
        if (style != null) button.Style = style;
        if (click != null) button.Click += click;
        return button;
    }

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="glyph">The icon glyph.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(string name, char glyph, RoutedEventHandler click, Style style = null)
        => CreateAppBarButton(name, new FontIcon
        {
            Glyph = glyph.ToString()
        }, click, style);

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="symbol">The icon symbol.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(string name, Symbol symbol, RoutedEventHandler click, Style style = null)
        => CreateAppBarButton(name, new SymbolIcon
        {
            Symbol = symbol
        }, click, style);

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="icon">The icon URI.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="showAsMonochrome">true if need to show the bitmap in a single color; otherwise, show the bitmap in full color.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(string name, Uri icon, RoutedEventHandler click, bool showAsMonochrome = false, Style style = null)
        => CreateAppBarButton(name, new BitmapIcon
        {
            UriSource = icon,
            ShowAsMonochrome = showAsMonochrome
        }, click, style);

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(string name, IconElement icon, Action click, Style style = null)
        => CreateAppBarButton(name, icon, click != null ? (sender, args) => click() : null, style);

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="glyph">The icon glyph.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(string name, char glyph, Action click, Style style = null)
        => CreateAppBarButton(name, glyph, click != null ? (sender, args) => click() : null, style);

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="symbol">The icon symbol.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(string name, Symbol symbol, Action click, Style style = null)
        => CreateAppBarButton(name, symbol, click != null ? (sender, args) => click() : null, style);

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="icon">The icon URI.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="showAsMonochrome">true if need to show the bitmap in a single color; otherwise, show the bitmap in full color.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(string name, Uri icon, Action click, bool showAsMonochrome = false, Style style = null)
        => CreateAppBarButton(name, icon, click != null ? (sender, args) => click() : null, showAsMonochrome, style);

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="bar">The command bar.</param>
    /// <param name="secondary">true if add to the secondary commands; otherwise, false.</param>
    /// <param name="name">The name.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(CommandBar bar, bool secondary, string name, IconElement icon, RoutedEventHandler click, Style style = null)
    {
        var c = CreateAppBarButton(name, icon, click, style);
        if (bar != null)
        {
            if (secondary) bar.SecondaryCommands.Add(c);
            else bar.PrimaryCommands.Add(c);
        }

        return c;
    }

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="bar">The command bar.</param>
    /// <param name="secondary">true if add to the secondary commands; otherwise, false.</param>
    /// <param name="name">The name.</param>
    /// <param name="glyph">The icon glyph.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(CommandBar bar, bool secondary, string name, char glyph, RoutedEventHandler click, Style style = null)
    {
        var c = CreateAppBarButton(name, glyph, click, style);
        if (bar != null)
        {
            if (secondary) bar.SecondaryCommands.Add(c);
            else bar.PrimaryCommands.Add(c);
        }

        return c;
    }

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="bar">The command bar.</param>
    /// <param name="secondary">true if add to the secondary commands; otherwise, false.</param>
    /// <param name="name">The name.</param>
    /// <param name="symbol">The icon symbol.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(CommandBar bar, bool secondary, string name, Symbol symbol, RoutedEventHandler click, Style style = null)
    {
        var c = CreateAppBarButton(name, symbol, click, style);
        if (bar != null)
        {
            if (secondary) bar.SecondaryCommands.Add(c);
            else bar.PrimaryCommands.Add(c);
        }

        return c;
    }

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="bar">The command bar.</param>
    /// <param name="secondary">true if add to the secondary commands; otherwise, false.</param>
    /// <param name="name">The name.</param>
    /// <param name="icon">The icon URI.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="showAsMonochrome">true if need to show the bitmap in a single color; otherwise, show the bitmap in full color.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(CommandBar bar, bool secondary, string name, Uri icon, RoutedEventHandler click, bool showAsMonochrome = false, Style style = null)
    {
        var c = CreateAppBarButton(name, icon, click, showAsMonochrome, style);
        if (bar != null)
        {
            if (secondary) bar.SecondaryCommands.Add(c);
            else bar.PrimaryCommands.Add(c);
        }

        return c;
    }

    /// <summary>
    /// Creates a button for command bar.
    /// </summary>
    /// <param name="bar">The command bar.</param>
    /// <param name="secondary">true if add to the secondary commands; otherwise, false.</param>
    /// <param name="name">The name.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="click">The click event handler.</param>
    /// <param name="style">The optional style of the button.</param>
    /// <returns>The button.</returns>
    public static AppBarButton CreateAppBarButton(CommandBar bar, bool secondary, string name, IconElement icon, Action click, Style style = null)
    {
        var c = CreateAppBarButton(name, icon, click != null ? (sender, args) => click() : null, style);
        if (bar != null)
        {
            if (secondary) bar.SecondaryCommands.Add(c);
            else bar.PrimaryCommands.Add(c);
        }

        return c;
    }

    /// <summary>
    /// Creates an SVG image.
    /// </summary>
    /// <param name="uri">The URI of the SVG.</param>
    /// <returns>The image element.</returns>
    public static SvgImageSource CreateSvgImageSourceByUri(Uri uri)
        => uri == null ? null : new()
        {
            UriSource = uri
        };

    /// <summary>
    /// Creates an SVG image.
    /// </summary>
    /// <param name="url">The URL of the SVG.</param>
    /// <returns>The image element.</returns>
    public static SvgImageSource CreateSvgImageSourceByUri(string url)
        => string.IsNullOrWhiteSpace(url) ? null : new()
        {
            UriSource = TryCreateUri(url)
        };

    /// <summary>
    /// Creates an SVG image.
    /// </summary>
    /// <param name="svgString">The SVG content string.</param>
    /// <returns>The image element.</returns>
    public static async Task<SvgImageSource> CreateSvgImageByCodeAsync(string svgString)
    {
        if (string.IsNullOrWhiteSpace(svgString)) return null;
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        await writer.WriteAsync(svgString);
        await writer.FlushAsync();
        stream.Position = 0;
        var svg = new SvgImageSource();
        await svg.SetSourceAsync(stream.AsRandomAccessStream());
        return svg;
    }

    /// <summary>
    /// Gets the SVG size from the source.
    /// </summary>
    /// <param name="svgString">The SVG content string.</param>
    /// <returns>The size of the SVG element; or null, if no such information.</returns>
    public static Size? GetSvgSize(string svgString)
        => GetSvgSize(svgString, out var width, out var height) ? new (width, height) : null;

    /// <summary>
    /// Gets the SVG size from the source.
    /// </summary>
    /// <param name="svgString">The SVG content string.</param>
    /// <param name="width">The width of the SVG element; or double.NaN if parses failed.</param>
    /// <param name="height">The height of the SVG element; or double.NaN if parses failed.</param>
    /// <returns>true if parses succeeded; otherwise, false.</returns>
    public static bool GetSvgSize(string svgString, out double width, out double height)
    {
        var svg = XDocument.Parse(svgString).Root;
        var heightAttribute = svg.Attribute("height");
        var widthAttribute = svg.Attribute("width");
        if (heightAttribute.Value == null || !double.TryParse(heightAttribute.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out height))
            height = double.NaN;
        if (widthAttribute.Value == null || !double.TryParse(widthAttribute.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out width))
            width = double.NaN;
        return !double.IsNaN(width) && !double.IsNaN(height) && !double.IsInfinity(width) && !double.IsInfinity(height);
    }

    /// <summary>
    /// Tries to get tag of the element.
    /// </summary>
    /// <typeparam name="T">The type of tag.</typeparam>
    /// <param name="target">The target element.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if gets succeeded; otherwise, false.</returns>
    public static bool TryGetTag<T>(object target, out T result)
    {
        if (target is FrameworkElement element && element.Tag is T tag)
        {
            result = tag;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Tries to get tag of the element.
    /// </summary>
    /// <typeparam name="TTag">The type of tag.</typeparam>
    /// <typeparam name="TElement">The type of element.</typeparam>
    /// <param name="target">The target element.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if gets succeeded; otherwise, false.</returns>
    public static bool TryGetTag<TTag, TElement>(object target, out TTag result) where TElement : FrameworkElement
    {
        if (target is TElement element && element.Tag is TTag tag)
        {
            result = tag;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Tries to get tag of the element.
    /// </summary>
    /// <typeparam name="T">The type of tag.</typeparam>
    /// <param name="target">The target element.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if gets succeeded; otherwise, false.</returns>
    public static bool TryGetDataContext<T>(object target, out T result)
    {
        if (target is FrameworkElement element && element.DataContext is T tag)
        {
            result = tag;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Tries to get tag of the element.
    /// </summary>
    /// <typeparam name="TTag">The type of tag.</typeparam>
    /// <typeparam name="TElement">The type of element.</typeparam>
    /// <param name="target">The target element.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if gets succeeded; otherwise, false.</returns>
    public static bool TryGetDataContext<TTag, TElement>(object target, out TTag result) where TElement : FrameworkElement
    {
        if (target is TElement element && element.DataContext is TTag tag)
        {
            result = tag;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Tries to parse a color.
    /// </summary>
    /// <param name="s">The input string to parse.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if parse succeeded; otherwise, false.</returns>
    public static bool TryParseColor(string s, out Color result)
    {
        if (!Drawing.ColorCalculator.TryParse(s, out var color))
        {
            result = default;
            return false;
        }

        result = Color.FromArgb(color.A, color.R, color.G, color.B);
        return true;
    }

    /// <summary>
    /// Parses a color.
    /// </summary>
    /// <param name="s">The input string to parse.</param>
    /// <returns>The result color parsed.</returns>
    /// <exception cref="FormatException">s was incorrect to parse as a color.</exception>
    public static Color ParseColor(string s)
    {
        s = s.Trim();
        if (string.IsNullOrEmpty(s)) return Microsoft.UI.Colors.Transparent;
        if (TryParseColor(s, out var r)) return r;
        throw new FormatException("s is not a color.", new ArgumentException("s is not supported.", nameof(s)));
    }

    /// <summary>
    /// Converts a color to hex format string.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <returns>A hex format string.</returns>
    public static string ToHexString(Color value)
        => value.A == 255 ? $"#{value.R:x2}{value.G:x2}{value.B:x2}" : $"#{value.A:x2}{value.R:x2}{value.G:x2}{value.B:x2}";

    /// <summary>
    /// Converts a color to hex format string.
    /// </summary>
    /// <param name="value">The source color value.</param>
    /// <returns>A hex format string.</returns>
    public static string ToRgbaString(Color value)
        => $"rgba({value.R}, {value.G}, {value.B}, {value.A / 255d:0.######})";

    /// <summary>
    /// Tries to parse a URI.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The URI.</returns>
    public static Uri TryCreateUri(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            return new Uri(url);
        }
        catch (FormatException)
        {
        }
        catch (ArgumentException)
        {
        }

        return null;
    }

    /// <summary>
    /// Adds object to JS environment.
    /// </summary>
    /// <param name="webview">The web view.</param>
    /// <param name="name">The property name.</param>
    /// <param name="json">The value.</param>
    public static void AddExternalObjectToScript(WebView2 webview, string name, JsonObjectNode json)
        => _ = AddExternalObjectToScriptAsync(webview, name, json);

    /// <summary>
    /// Adds object to JS environment.
    /// </summary>
    /// <param name="webview">The web view.</param>
    /// <param name="name">The property name.</param>
    /// <param name="json">The value.</param>
    public static void AddHostObjectToScript(WebView2 webview, string name, JsonObjectNode json)
        => _ = AddHostObjectToScriptAsync(webview, name, json);

    /// <summary>
    /// Adds object to JS environment.
    /// </summary>
    /// <param name="webview">The web view.</param>
    /// <param name="name">The property name.</param>
    /// <param name="json">The value.</param>
    public static async Task AddExternalObjectToScriptAsync(WebView2 webview, string name, JsonObjectNode json)
    {
        if (string.IsNullOrEmpty(name) || json == null || webview == null) return;
        await webview.EnsureCoreWebView2Async();
        var sb = new StringBuilder();
        sb.Append(@"(function() { if (!window.external) window.external = { }; window.external.");
        sb.Append(name);
        sb.Append(" = ");
        sb.Append(json.ToString());
        sb.Append("; })();");
        await webview.CoreWebView2.ExecuteScriptAsync(sb.ToString());
    }

    /// <summary>
    /// Adds object to JS environment.
    /// </summary>
    /// <param name="webview">The web view.</param>
    /// <param name="name">The property name.</param>
    /// <param name="json">The value.</param>
    public static async Task AddHostObjectToScriptAsync(WebView2 webview, string name, JsonObjectNode json)
    {
        if (string.IsNullOrEmpty(name) || json == null || webview == null) return;
        await webview.EnsureCoreWebView2Async();
        var sb = new StringBuilder();
        sb.Append(@"(function() { if (!window.chrome) window.chrome = {}; if (!window.chrome.webview) window.chrome.webview = {}; if (!window.chrome.webview.hostObjects) window.chrome.webview.hostObjects = {}; window.chrome.webview.hostObjects.");
        sb.Append(name);
        sb.Append(" = ");
        sb.Append(json.ToString());
        sb.Append("; })();");
        await webview.CoreWebView2.ExecuteScriptAsync(sb.ToString());
    }

    /// <summary>
    /// Applies mica system backdrop.
    /// </summary>
    /// <param name="window">The window to enable the effect.</param>
    /// <param name="theme">The request theme.</param>
    /// <param name="backdropController">The backdrop controller maker.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static void ApplyMicaSystemBackdrop(Window window, ElementTheme? theme = null, Func<ElementTheme, ISystemBackdropControllerWithTargets> backdropController = null, CancellationToken cancellationToken = default)
    {
        if (window == null) return;
        var backdrop = new SystemBackdropClient
        {
            BackdropControllerMaker = backdropController
        };
        theme ??= (window.Content as FrameworkElement)?.RequestedTheme;
        backdrop.UpdateWindowBackground(window, theme ?? ElementTheme.Default);
        TypedEventHandler<object, WindowActivatedEventArgs> activated = (sender, ev) =>
        {
            try
            {
                backdrop.IsInputActive = ev.WindowActivationState != WindowActivationState.Deactivated;
            }
            catch (InvalidOperationException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ExternalException)
            {
            }
        };
        TypedEventHandler<object, WindowEventArgs> closed = (sender, ev) =>
        {
            try
            {
                backdrop.Dispose();
            }
            catch (InvalidOperationException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ExternalException)
            {
            }
        };
        if (cancellationToken.IsCancellationRequested) return;
        window.Activated += activated;
        window.Closed += closed;
        try
        {
            if (cancellationToken.CanBeCanceled) cancellationToken.Register(() =>
            {
                try
                {
                    window.Activated -= activated;
                    window.Closed -= closed;
                }
                catch (InvalidOperationException)
                {
                }
                catch (NullReferenceException)
                {
                }
            });
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
    }

    /// <summary>
    /// Applies mica system backdrop.
    /// </summary>
    /// <param name="window">The window to enable the effect.</param>
    /// <param name="theme">The request theme.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static void ApplyMicaSystemBackdrop(Window window, ElementTheme? theme, CancellationToken cancellationToken)
        => ApplyMicaSystemBackdrop(window, theme, null, cancellationToken);

    /// <summary>
    /// Opens a file.
    /// </summary>
    /// <param name="file">The file info.</param>
    /// <returns>true if opens succeeded; otherwise, false.</returns>
    public static async Task<bool> OpenFileAsync(FileInfo file)
    {
        if (file == null || !file.Exists) return false;
        string path = null;
        try
        {
            path = file.FullName;
        }
        catch (IOException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ExternalException)
        {
        }

        if (string.IsNullOrEmpty(path)) return false;
        return await OpenFileAsync(path);
    }

    /// <summary>
    /// Opens a file.
    /// </summary>
    /// <param name="file">The file path.</param>
    /// <returns>true if opens succeeded; otherwise, false.</returns>
    public static async Task<bool> OpenFileAsync(string file)
    {
        try
        {
            var fileStorage = await StorageFile.GetFileFromPathAsync(file);
            if (fileStorage == null) return false;
            return await Launcher.LaunchFileAsync(fileStorage);
        }
        catch (IOException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ExternalException)
        {
        }

        return false;
    }

    /// <summary>
    /// Opens a directory.
    /// </summary>
    /// <param name="dir">The directory info.</param>
    /// <returns>true if opens succeeded; otherwise, false.</returns>
    public static async Task<bool> OpenFolderAsync(DirectoryInfo dir)
    {
        if (dir == null || !dir.Exists) return false;
        string path = null;
        try
        {
            path = dir.FullName;
        }
        catch (IOException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ExternalException)
        {
        }

        if (string.IsNullOrEmpty(path)) return false;
        return await Launcher.LaunchFolderPathAsync(path);
    }

    /// <summary>
    /// Runs the command and gets the output.
    /// </summary>
    /// <param name="fileName">An application with which to start a process.</param>
    /// <param name="args">Command-line arguments to pass to the application when the process starts.</param>
    /// <param name="callback">The callback when the start information object is initialized.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The lines of output.</returns>
    public static async Task<string> RunCommandForOutputAsync(string fileName, string args, Action<ProcessStartInfo> callback, CancellationToken cancellationToken = default)
    {
        using var process = CreateRedirectOutputProcess(fileName, args, callback);
        if (process == null) return null;
        var output = new StringBuilder();
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data is null) return;
            output.AppendLine(e.Data);
        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken);
        return output.ToString();
    }

    /// <summary>
    /// Runs the command and gets the output.
    /// </summary>
    /// <param name="fileName">An application with which to start a process.</param>
    /// <param name="args">Command-line arguments to pass to the application when the process starts.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The lines of output.</returns>
    public static Task<string> RunCommandForOutputAsync(string fileName, string args = null, CancellationToken cancellationToken = default)
        => RunCommandForOutputAsync(fileName, args, null, cancellationToken);

    /// <summary>
    /// Gets the port number of Foundry local service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The port number of Foundry local service; or -1, if fails to get.</returns>
    public static async Task<int> GetFoundryLocalPortAsync(CancellationToken cancellationToken = default)
    {
        var s = await RunCommandForOutputAsync("foundry", "service status", start =>
        {
            start.EnvironmentVariables["DOTNET_ENVIRONMENT"] = null;
        }, cancellationToken);
        if (string.IsNullOrWhiteSpace(s)) return -1;
        var i = s.IndexOf("http://");
        if (i < 0) return -1;
        s = s[i..];
        i = s.IndexOf('/', 10);
        s = s[..i];
        i = s.LastIndexOf(':');
        if (i < 1) return -1;
        s = s[(i + 1)..];
        return int.TryParse(s, out var port) ? port : -1;
    }

    /// <summary>
    /// Starts a specific Windows service.
    /// </summary>
    /// <param name="name">The service name.</param>
    /// <returns>The process; or null, if fails.</returns>
    public static Process StartWindowsService(string name)
        => ChangeWindowsServiceStatus(name, "start");

    /// <summary>
    /// Pauses a specific Windows service.
    /// </summary>
    /// <param name="name">The service name.</param>
    /// <returns>The process; or null, if fails.</returns>
    public static Process PauseWindowsService(string name)
        => ChangeWindowsServiceStatus(name, "pause");

    /// <summary>
    /// Stops a specific Windows service.
    /// </summary>
    /// <param name="name">The service name.</param>
    /// <returns>The process; or null, if fails.</returns>
    public static Process StopWindowsService(string name)
        => ChangeWindowsServiceStatus(name, "stop");

    internal static Process ChangeWindowsServiceStatus(string name, string action)
    {
        try
        {
            var info = new ProcessStartInfo("net.exe", string.Concat(action, ' ', name))
            {
                UseShellExecute = true,
                Verb = "runas"
            };
            return Process.Start(info);
        }
        catch (NotSupportedException)
        {
        }
        catch (IOException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    internal static DesktopAcrylicController TryCreateAcrylicBackdrop()
    {
        try
        {
            if (!DesktopAcrylicController.IsSupported()) return null;
            return new();
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    internal static MicaController TryCreateMicaBackdrop()
    {
        try
        {
            if (!MicaController.IsSupported()) return null;
            return new();
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    /// <summary>
    /// Create text view models.
    /// </summary>
    /// <param name="json">The data source.</param>
    /// <param name="start">The start index.</param>
    /// <param name="style">The optional style for JSON.</param>
    /// <returns>The text view model collection</returns>
    internal static List<TextViewModel> CreateTextViewModels(JsonObjectNode json, int start, JsonTextStyle style = null)
    {
        var arr = new TextViewModelFactory(start);
        if (json == null) return arr.Collection;
        CreateTextViewModels(arr, json, style ?? new(), 0);
        arr.PushLine();
        return arr.Collection;
    }

    /// <summary>
    /// Create text view models.
    /// </summary>
    /// <param name="json">The data source.</param>
    /// <param name="start">The start index.</param>
    /// <param name="style">The optional style for JSON.</param>
    /// <returns>The text view model collection</returns>
    internal static List<TextViewModel> CreateTextViewModels(JsonArrayNode json, int start, JsonTextStyle style = null)
    {
        var arr = new TextViewModelFactory(start);
        if (json == null) return arr.Collection;
        CreateTextViewModels(arr, json, style ?? new(), 0);
        arr.PushLine();
        return arr.Collection;
    }

    internal static JsonTextStyle GetDefaultJsonTextStyle(FrameworkElement element)
    {
        try
        {
            return element.ActualTheme switch
            {
                ElementTheme.Light => new(ApplicationTheme.Light),
                ElementTheme.Dark => new(ApplicationTheme.Dark),
                _ => null
            };
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    internal static async Task<DirectoryInfo> SelectFolderAsync(Window window, Action<ExternalException> onExternalExceptionThrow = null)
    {
        var picker = new Windows.Storage.Pickers.FolderPicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads,
        };
        try
        {
            if (window != null) WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) return IO.FileSystemInfoUtility.TryGetDirectoryInfo(folder.Path);
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (ExternalException ex)
        {
            onExternalExceptionThrow?.Invoke(ex);
        }

        return null;
    }

    internal static bool? ConvertToBoolean(object value)
    {
        if (value is null) return null;
        if (value is bool b) return b;
        if (value is Visibility v) return v == Visibility.Visible;
        if (value is int i) return i > 0;
        if (value is JsonBooleanNode j) return j.Value;
        if (value is string s) return JsonBooleanNode.TryParse(s)?.Value ?? false;
        if (value is UnaryBooleanOperator o) return o == UnaryBooleanOperator.Default;
        return null;
    }

    internal static object ConvertFromBoolean(bool? value, Type targetType, bool useUnsetValue)
    {
        if (!value.HasValue || targetType == null) return useUnsetValue ? DependencyProperty.UnsetValue : null;
        var b = value.Value;
        if (targetType == typeof(Visibility)) return b ? Visibility.Visible : Visibility.Collapsed;
        if (targetType == typeof(bool)) return b;
        if (targetType == typeof(int)) return b ? 1 : 0;
        if (targetType == typeof(JsonBooleanNode)) return (JsonBooleanNode)b;
        if (targetType == typeof(string)) return b ? JsonBooleanNode.TrueString : JsonBooleanNode.FalseString;
        if (targetType == typeof(UnaryBooleanOperator)) return b ? UnaryBooleanOperator.Default : UnaryBooleanOperator.Not;
        return useUnsetValue ? DependencyProperty.UnsetValue : null;
    }

    private static Process CreateRedirectOutputProcess(string fileName, string args, Action<ProcessStartInfo> callback)
    {
        try
        {
            var start = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            callback?.Invoke(start);
            return Process.Start(start);
        }
        catch (NotSupportedException)
        {
        }
        catch (IOException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    private static void CreateTextInlines(List<Inline> arr, JsonObjectNode json, JsonTextStyle style, int intend)
    {
        if (json == null) return;
        var blank = new string(' ', intend * (style.IsCompact ? 2 : 4));
        arr.Add(CreateRun('{', style.PunctuationForeground));
        var i = 0;
        var blank2 = string.Concat(blank, style.IsCompact ? "  " : "    ");
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
        var blank = new string(' ', intend * (style.IsCompact ? 2 : 4));
        arr.Add(CreateRun('[', style.PunctuationForeground));
        var i = 0;
        var blank2 = string.Concat(blank, style.IsCompact ? "  " : "    ");
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

    private static void CreateTextInlines(List<Inline> arr, IJsonValueNode json, JsonTextStyle style, int intend)
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

    private static void CreateTextViewModels(TextViewModelFactory arr, JsonObjectNode json, JsonTextStyle style, int intend)
    {
        if (json == null) return;
        var blank = new string(' ', intend * (style.IsCompact ? 2 : 4));
        arr.AppendToBuffer('{', style.PunctuationForeground);
        var i = 0;
        var blank2 = string.Concat(blank, style.IsCompact ? "  " : "    ");
        foreach (var prop in json)
        {
            if (prop.Value is null) continue;
            if (i > 0) arr.AppendToBuffer(',', style.PunctuationForeground);

            arr.PushLine();
            arr.AppendToBuffer(blank2, null);
            arr.AppendToBuffer(JsonStringNode.ToJson(prop.Key), style.PropertyForeground);
            arr.AppendToBuffer(": ", style.PunctuationForeground);
            CreateTextViewModels(arr, prop.Value, style, intend + 1);
            i++;
        }

        arr.PushLine();
        arr.AppendToBuffer(blank, null);
        arr.AppendToBuffer('}', style.PunctuationForeground);
    }

    private static void CreateTextViewModels(TextViewModelFactory arr, JsonArrayNode json, JsonTextStyle style, int intend)
    {
        if (json == null) return;
        var blank = new string(' ', intend * (style.IsCompact ? 2 : 4));
        arr.AppendToBuffer('[', style.PunctuationForeground);
        var i = 0;
        var blank2 = string.Concat(blank, style.IsCompact ? "  " : "    ");
        foreach (var item in json)
        {
            if (item is null) continue;
            if (i > 0) arr.AppendToBuffer(',', style.PunctuationForeground);
            arr.PushLine();
            arr.AppendToBuffer(blank2, null);
            CreateTextViewModels(arr, item, style, intend + 1);
            i++;
        }

        arr.PushLine();
        arr.AppendToBuffer(blank, null);
        arr.AppendToBuffer(']', style.PunctuationForeground);
    }

    private static void CreateTextViewModels(TextViewModelFactory arr, IJsonValueNode json, JsonTextStyle style, int intend)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                arr.AppendToBuffer("null", style.KeywordForeground);
                break;
            case JsonValueKind.String:
                arr.AppendToBuffer(json.ToString(), style.StringForeground);
                break;
            case JsonValueKind.Number:
                arr.AppendToBuffer(json.ToString(), style.NumberForeground);
                break;
            case JsonValueKind.True:
                arr.AppendToBuffer("true", style.KeywordForeground);
                break;
            case JsonValueKind.False:
                arr.AppendToBuffer("false", style.KeywordForeground);
                break;
            case JsonValueKind.Object:
                CreateTextViewModels(arr, json as JsonObjectNode, style, intend);
                break;
            case JsonValueKind.Array:
                CreateTextViewModels(arr, json as JsonArrayNode, style, intend);
                break;
            default:
                break;
        }
    }
}
