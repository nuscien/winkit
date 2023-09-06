using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WebUI;

namespace Trivial.UI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LocalWebAppPage : Page
{
    private TabbedWebViewWindow tabbedWebViewWindowInstance;

    /// <summary>
    /// Gets or sets the monitor.
    /// </summary>
    public static ILocalWebAppPageMonitor MonitorSingleton { get; set; }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppPage class.
    /// </summary>
    public LocalWebAppPage()
    {
        InitializeComponent();
        messageHandler = new(Browser);
        MonitorSingleton?.OnCreate(this, Browser);
        if (!string.IsNullOrWhiteSpace(LocalWebAppSettings.CustomizedLocaleStrings.Continue)) CloseInfoButton.Content = LocalWebAppSettings.CustomizedLocaleStrings.Continue;
    }

    /// <summary>
    /// Gets or sets the default background color of the browser.
    /// </summary>
    public Windows.UI.Color BrowserBackgroundColor
    {
        get => Browser.DefaultBackgroundColor;
        set => Browser.DefaultBackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the element to draw the background.
    /// </summary>
    public UIElement BackgroundElement
    {
        get => BackgroundContainer.Child;
        set => BackgroundContainer.Child = value;
    }

    /// <summary>
    /// Load a dev local web app.
    /// </summary>
    /// <param name="window">The host window.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The async task.</returns>
    public async Task LoadDevPackageAsync(Window window, CancellationToken cancellationToken = default)
    {
        var dir = await VisualUtility.SelectFolderAsync(window) ?? throw new InvalidOperationException("Requires a directory.", new OperationCanceledException("No directory selected."));
        cancellationToken.ThrowIfCancellationRequested();
        await LoadDevPackageAsync(dir, cancellationToken);
    }

    /// <summary>
    /// Focuses the browser.
    /// </summary>
    /// <param name="value">The focus state.</param>
    public void FocusBrowser(FocusState value)
        => Browser.Focus(value);

    /// <summary>
    /// Stops progress ring.
    /// </summary>
    public void StopLoading()
        => ProgressElement.IsActive = false;

    /// <summary>
    /// Shows notification.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="message">The message.</param>
    /// <param name="severity">The severity.</param>
    public void ShowNotification(string title, string message, InfoBarSeverity severity)
    {
        NotificationBar.Title = title;
        NotificationBar.Message = message;
        NotificationBar.Severity = severity;
        NotificationBar.IsOpen = true;
        ProgressElement.IsActive = false;
    }

    /// <inheritdoc />
    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        Browser.Close();
        MonitorSingleton?.OnNavigatingFrom(this, e);
    }

    /// <inheritdoc />
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        MonitorSingleton?.OnNavigatedFrom(this, e);
    }

    /// <inheritdoc />
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is LocalWebAppHost host)
            _ = LoadAsync(host);
        else if (e.Parameter is Task<LocalWebAppHost> hostTask)
            _ = LoadAsync(hostTask);
        else if (e.Parameter is LocalWebAppOptions options)
            _ = LoadAsync(options);
        else if (e.Parameter is string id)
            _ = LoadAsync(id);
        else if (e.Parameter is DirectoryInfo dir)
            _ = LoadDevPackageAsync(dir);
        else if (e.Parameter is IPageNavigationCallbackParameter<LocalWebAppPage> c)
            c.OnNavigate(this, e);
        MonitorSingleton?.OnNavigatedTo(this, e);
    }

    private void SetInfoViewContainerVisibility(bool value)
        => InfoViewContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;

    private void SetBrowserVisibility(bool value)
        => Browser.Visibility = value ? Visibility.Visible : Visibility.Collapsed;

    private void OnLoadError(Exception ex)
    {
        if (ex == null) return;
        var title = string.IsNullOrWhiteSpace(LocalWebAppSettings.CustomizedLocaleStrings.ErrorTitle) ? "Error" : LocalWebAppSettings.CustomizedLocaleStrings.ErrorTitle;
        var message = ex is LocalWebAppSignatureException signEx ? LocalWebAppSettings.SignErrorMessage?.Invoke(signEx) : null;
        NotificationBar.Title = title;
        NotificationBar.Message = string.IsNullOrWhiteSpace(message) ? ex?.Message : message;
        NotificationBar.Severity = InfoBarSeverity.Error;
        NotificationBar.IsOpen = true;
        ProgressElement.IsActive = false;
        MonitorSingleton?.OnErrorNotification(this, NotificationBar, ex);
        LoadFailed?.Invoke(this, new DataEventArgs<Exception>(ex));
        TitleChanged?.Invoke(this, new DataEventArgs<string>(title));
    }

    internal static async Task EnsureRun(WebView2 webview, Action<CoreWebView2> handler)
    {
        if (webview is null) return;
        await webview.EnsureCoreWebView2Async();
        handler?.Invoke(webview.CoreWebView2);
    }

    internal static async Task<bool> OnNewWindowRequestedAsync(CoreWebView2NewWindowRequestedEventArgs args, TabbedWebViewWindow window, Action<CoreWebView2> callback, ElementTheme theme)
    {
        var uri = VisualUtility.TryCreateUri(args.Uri);
        if (uri == null) return false;
        args.Handled = true;
        var deferral = args.GetDeferral();
        if (window == null) return false;
        var webview = window.Add(uri);
        await webview.EnsureCoreWebView2Async();
        args.NewWindow = webview.CoreWebView2;
        callback?.Invoke(webview.CoreWebView2);
        deferral.Complete();
        window.TabView.WebViewTabCreated += (sender2, args2) =>
        {
            _ = EnsureRun(args2.WebView?.WebView2, callback);
        };
        try
        {
            window.TabView.RequestedTheme = theme;
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ApplicationException)
        {
        }
        catch (ExternalException)
        {
        }

        return true;
    }

    private async Task OnNewWindowRequestedAsync(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        var uri = VisualUtility.TryCreateUri(args.Uri);
        if (uri == null) return;
        args.Handled = true;
        var window = tabbedWebViewWindowInstance;
        if (tabbedWebViewWindowInstance == null)
        {
            window = new TabbedWebViewWindow
            {
                IsReadOnly = true,
                DownloadList = DownloadList
            };
            window.Closed += (sender2, args2) =>
            {
                tabbedWebViewWindowInstance = null;
            };
            OnWindowCreate?.Invoke(window);
        }

        await OnNewWindowRequestedAsync(args, window, OnWebViewTabInitialized, Browser.RequestedTheme);
        tabbedWebViewWindowInstance = window;
        window.Activate();
    }

    private void OnActualThemeChanged(FrameworkElement sender, object args)
        => Notify(null as string, "themeChanged", new(messageHandler?.GetTheme()));

    private void OnCloseInfoButton(object sender, RoutedEventArgs e)
    {
        InfoViewContainer.Visibility = Visibility.Collapsed;
        InfoView.Model = null;
        var h = continueHandler;
        continueHandler = null;
        h?.Invoke();
    }
}
