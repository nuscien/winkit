using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Maths;
using Trivial.Text;
using Trivial.UI;
using Windows.Graphics;

namespace Trivial.Web;

/// <summary>
/// The browser message handler for local standalone web app.
/// </summary>
public interface ILocalWebAppBrowserMessageHandler
{
    /// <summary>
    /// Gets download list information.
    /// </summary>
    /// <param name="open">true if open the default dialog; false if hide; or null, no action.</param>
    /// <param name="maxCount">The maximum count of download item to return.</param>
    /// <returns>The result.</returns>
    JsonObjectNode DownloadListInfo(bool? open, int maxCount = 256);

    /// <summary>
    /// Gets light or dark information.
    /// </summary>
    /// <returns>The result.</returns>
    JsonObjectNode GetTheme();

    /// <summary>
    /// Focuses the browser.
    /// </summary>
    void Focus();
}

/// <summary>
/// The controller of window.
/// </summary>
public interface IBasicWindowStateController
{
    /// <summary>
    /// Gets the window title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets window size.
    /// </summary>
    /// <param name="physicalPixel">true if return the value in physical pixel; otherwise, false.</param>
    /// <returns>The window size.</returns>
    IntPoint2D Size(bool physicalPixel = false);

    /// <summary>
    /// Sets the window size.
    /// </summary>
    /// <param name="w">The width.</param>
    /// <param name="h">The height.</param>
    /// <param name="physicalPixel">true if return the value in physical pixel; otherwise, false.</param>
    void Size(int w, int h, bool physicalPixel = false);

    /// <summary>
    /// Gets the window position.
    /// </summary>
    /// <returns>The position of top-left corner.</returns>
    /// <param name="physicalPixel">true if return the value in physical pixel; otherwise, false.</param>
    IntPoint2D Position(bool physicalPixel = false);

    /// <summary>
    /// Sets the window position.
    /// </summary>
    /// <param name="x">Left.</param>
    /// <param name="y">Top.</param>
    /// <param name="physicalPixel">true if return the value in physical pixel; otherwise, false.</param>
    void Position(int x, int y, bool physicalPixel = false);

    /// <summary>
    /// Gets the window state.
    /// </summary>
    /// <returns>The window state.</returns>
    CommonWindowStates WindowState();

    /// <summary>
    /// Restores the window.
    /// </summary>
    /// <returns>true if succeed; otherwise, false.</returns>
    bool Restore();

    /// <summary>
    /// Maximizes the window.
    /// </summary>
    /// <returns>true if succeed; otherwise, false.</returns>
    bool Maximize();

    /// <summary>
    /// Minimizes the window.
    /// </summary>
    /// <returns>true if succeed; otherwise, false.</returns>
    bool Minimize();

    /// <summary>
    /// Enters to or exits from full screen mode.
    /// </summary>
    /// <param name="value">true if to enter to full screen mode; otherwise, false.</param>
    void FullScreen(bool value);
}

internal class LocalWebAppBrowserMessageHandler : ILocalWebAppBrowserMessageHandler
{
    private readonly WebView2 webview;

    /// <summary>
    /// Initializes a new instance of the LocalWebAppBrowserMessageHandler class.
    /// </summary>
    /// <param name="webview">The web view element.</param>
    public LocalWebAppBrowserMessageHandler(WebView2 webview)
    {
        this.webview = webview;
    }

    public List<CoreWebView2DownloadOperation> DownloadList { get; } = new();

    /// <summary>
    /// Gets download list information.
    /// </summary>
    /// <param name="open">true if open the default dialog; false if hide; or null, no action.</param>
    /// <param name="maxCount">The maximum count of download item to return.</param>
    public JsonObjectNode DownloadListInfo(bool? open, int maxCount = 256)
    {
        if (open.HasValue)
        {
            if (open.Value) webview.CoreWebView2.OpenDefaultDownloadDialog();
            else webview.CoreWebView2.CloseDefaultDownloadDialog();
        }

        var arr = new JsonArrayNode();
        foreach (var item in DownloadList)
        {
            arr.Add(new JsonObjectNode
            {
                { "uri", item.Uri },
                { "file", item.ResultFilePath },
                { "state", item.State.ToString() },
                { "received", item.BytesReceived },
                { "length", item.TotalBytesToReceive },
                { "interrupt", item.InterruptReason.ToString() },
                { "mime", item.MimeType }
            });
        }

        return new JsonObjectNode
        {
            { "dialog", open ?? webview.CoreWebView2.IsDefaultDownloadDialogOpen },
            { "max", maxCount },
            { "list", arr },
            { "enumerated", DateTime.Now }
        };
    }

    /// <summary>
    /// Gets light or dark information.
    /// </summary>
    /// <returns>The result.</returns>
    public JsonObjectNode GetTheme()
    {
        try
        {
            return new JsonObjectNode
            {
                { "brightness", Application.Current.RequestedTheme.ToString().ToLowerInvariant() }
            };
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (ExternalException)
        {
        }
        catch (NullReferenceException)
        {
        }

        return null;
    }

    /// <summary>
    /// Focuses the browser.
    /// </summary>
    public void Focus()
    {
        try
        {
            webview.Focus(FocusState.Programmatic);
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (ExternalException)
        {
        }
        catch (NullReferenceException)
        {
        }
    }
}

/// <summary>
/// The controller of window.
/// </summary>
public class BasicWindowStateController : IBasicWindowStateController
{
    private readonly Window win;
    private readonly AppWindow appWin;

    /// <summary>
    /// Initializes a new instance of the WindowStateController class.
    /// </summary>
    /// <param name="window">The window.</param>
    public BasicWindowStateController(Window window)
    {
        win = window;
        appWin = VisualUtility.TryGetAppWindow(window);
    }

    /// <summary>
    /// Gets the window title.
    /// </summary>
    public string Title => appWin.Title;

    /// <summary>
    /// Gets window size.
    /// </summary>
    /// <returns>The window size.</returns>
    /// <param name="physicalPixel">true if return the value in physical pixel; otherwise, false.</param>
    public IntPoint2D Size(bool physicalPixel = false)
    {
        if (physicalPixel)
        {
            var size = appWin.ClientSize;
            return new(size.Width, size.Height);
        }
        else
        {
            var size = win.Bounds;
            return new((int)size.Width, (int)size.Height);
        }
    }

    /// <summary>
    /// Sets the window size.
    /// </summary>
    /// <param name="w">The width.</param>
    /// <param name="h">The height.</param>
    /// <param name="physicalPixel">true if return the value in physical pixel; otherwise, false.</param>
    public void Size(int w, int h, bool physicalPixel = false)
    {
        if (!physicalPixel)
        {
            var dw = GetDpiX(true);
            var dh = GetDpiY(true);
            w = (int)Math.Round(w * dw);
            h = (int)Math.Round(h * dh);
        }

        appWin.ResizeClient(new(w, h));
    }

    /// <summary>
    /// Gets the window position.
    /// </summary>
    /// <returns>The position of top-left corner.</returns>
    /// <param name="physicalPixel">true if return the value in physical pixel; otherwise, false.</param>
    public IntPoint2D Position(bool physicalPixel = false)
    {
        var position = appWin.Position;
        if (physicalPixel) return new(position.X, position.Y);
        var dw = GetDpiX(false);
        var dh = GetDpiY(false);
        return new((int)Math.Round(position.X / dw), (int)Math.Round(position.Y / dh));
    }

    /// <summary>
    /// Sets the window position.
    /// </summary>
    /// <param name="x">Left.</param>
    /// <param name="y">Top.</param>
    /// <param name="physicalPixel">true if return the value in physical pixel; otherwise, false.</param>
    public void Position(int x, int y, bool physicalPixel = false)
    {
        if (!physicalPixel)
        {
            var dw = GetDpiX(true);
            var dh = GetDpiY(true);
            x = (int)Math.Round(x * dw);
            y = (int)Math.Round(y * dh);
        }

        appWin.Move(new(x, y));
    }

    /// <summary>
    /// Gets the window state.
    /// </summary>
    /// <returns>The window state.</returns>
    public CommonWindowStates WindowState()
    {
        try
        {
            var presenter = appWin.Presenter;
            if (presenter == null) return CommonWindowStates.Unknown;
            switch (presenter.Kind)
            {
                case AppWindowPresenterKind.CompactOverlay:
                    return CommonWindowStates.Compact;
                case AppWindowPresenterKind.FullScreen:
                    return CommonWindowStates.Fullscreen;
            }

            if (presenter is not OverlappedPresenter p) return CommonWindowStates.Unknown;
            return p.State switch
            {
                OverlappedPresenterState.Restored => CommonWindowStates.Restored,
                OverlappedPresenterState.Maximized => CommonWindowStates.Maximized,
                OverlappedPresenterState.Minimized => CommonWindowStates.Minimized,
                _ => CommonWindowStates.Restored
            };
        }
        catch (ExternalException)
        {
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

        return CommonWindowStates.Unknown;
    }

    /// <summary>
    /// Restores the window.
    /// </summary>
    /// <returns>true if succeed; otherwise, false.</returns>
    public bool Restore()
    {
        try
        {
            var presenter = appWin.Presenter as OverlappedPresenter;
            if (presenter == null && appWin.Presenter != null && appWin.Presenter.Kind != AppWindowPresenterKind.Overlapped && appWin.Presenter.Kind != AppWindowPresenterKind.Default)
            {
                appWin.SetPresenter(AppWindowPresenterKind.Default);
                presenter = appWin.Presenter as OverlappedPresenter;
            }

            if (presenter == null) return false;
            if (presenter.State != OverlappedPresenterState.Restored) presenter.Restore();
            return true;
        }
        catch (ExternalException)
        {
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

        return false;
    }

    /// <summary>
    /// Maximizes the window.
    /// </summary>
    /// <returns>true if succeed; otherwise, false.</returns>
    public bool Maximize()
    {
        try
        {
            var presenter = appWin.Presenter as OverlappedPresenter;
            if (presenter == null && appWin.Presenter != null && appWin.Presenter.Kind != AppWindowPresenterKind.Overlapped && appWin.Presenter.Kind != AppWindowPresenterKind.Default)
            {
                appWin.SetPresenter(AppWindowPresenterKind.Default);
                presenter = appWin.Presenter as OverlappedPresenter;
            }

            if (presenter == null || !presenter.IsMaximizable) return false;
            if (presenter.State != OverlappedPresenterState.Maximized) presenter.Maximize();
            return true;
        }
        catch (ExternalException)
        {
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

        return false;
    }

    /// <summary>
    /// Minimizes the window.
    /// </summary>
    /// <returns>true if succeed; otherwise, false.</returns>
    public bool Minimize()
    {
        try
        {
            var presenter = appWin.Presenter as OverlappedPresenter;
            if (presenter == null && appWin.Presenter != null && appWin.Presenter.Kind != AppWindowPresenterKind.Overlapped && appWin.Presenter.Kind != AppWindowPresenterKind.Default)
            {
                appWin.SetPresenter(AppWindowPresenterKind.Default);
                presenter = appWin.Presenter as OverlappedPresenter;
            }

            if (presenter == null || !presenter.IsMinimizable) return false;
            if (presenter.State != OverlappedPresenterState.Minimized) presenter.Minimize();
            return true;
        }
        catch (ExternalException)
        {
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

        return false;
    }

    /// <summary>
    /// Enters to or exits from full screen mode.
    /// </summary>
    /// <param name="value">true if to enter to full screen mode; otherwise, false.</param>
    public void FullScreen(bool value)
        => VisualUtility.SetFullScreenMode(value, appWin);

    private double GetDpiX(bool blur)
    {
        var r = appWin.ClientSize.Width / win.Bounds.Width;
        if (r < 0.2) return 1;
        return blur ? r : Math.Round(r * 100) / 100;
    }

    private double GetDpiY(bool blur)
    {
        var r = appWin.ClientSize.Height / win.Bounds.Height;
        if (r < 0.2) return 1;
        return blur ? r : Math.Round(r * 100) / 100;
    }
}
