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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Text;
using Trivial.UI;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.UI;

/// <summary>
/// The local standalone web app window.
/// </summary>
public sealed partial class LocalWebAppWindow : Window
{
    private readonly SystemBackdropClient backdrop;

    /// <summary>
    /// Initializes a new instance of the LocalWebAppWindow class.
    /// </summary>
    public LocalWebAppWindow()
    {
        InitializeComponent();
        try
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBackground);
        }
        catch (ArgumentException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        var theme = MainElement.RequestedTheme;
        backdrop = new();
        backdrop.UpdateWindowBackground(this, theme);
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppWindow class.
    /// </summary>
    public LocalWebAppWindow(LocalWebAppOptions options) : this()
        => _ = MainElement.LoadAsync(options);

    /// <summary>
    /// Occurs when the web view core processes failed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppWindow, CoreWebView2ProcessFailedEventArgs> CoreProcessFailed;

    /// <summary>
    /// Occurs when the web view core has initialized.
    /// </summary>
    public event TypedEventHandler<LocalWebAppWindow, CoreWebView2InitializedEventArgs> CoreWebView2Initialized;

    /// <summary>
    /// Occurs when the navigation is completed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppWindow, CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;

    /// <summary>
    /// Occurs when the navigation is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppWindow, CoreWebView2NavigationStartingEventArgs> NavigationStarting;

    /// <summary>
    /// Occurs when the new window request sent.
    /// </summary>
    public event TypedEventHandler<LocalWebAppWindow, CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested;

    /// <summary>
    /// Occurs when the webpage send request to close itself.
    /// </summary>
    public event TypedEventHandler<LocalWebAppWindow, object> WindowCloseRequested;

    /// <summary>
    /// Occurs when the downloading is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppWindow, CoreWebView2DownloadStartingEventArgs> DownloadStarting;

    /// <summary>
    /// Gets the download list.
    /// </summary>
    public List<CoreWebView2DownloadOperation> DownloadList => MainElement.DownloadList;

    /// <summary>
    /// Gets or sets a value indicating whether it is in debug mode to ignore any signature verification and enable Microsoft Edge DevTools.
    /// </summary>
    public bool IsDevEnvironmentEnabled
    {
        get => MainElement.IsDevEnvironmentEnabled;
        set => MainElement.IsDevEnvironmentEnabled = value;
    }

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="options">The options of the standalone web app.</param>
    public Task LoadAsync(LocalWebAppOptions options)
        => MainElement.LoadAsync(options);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    public Task LoadAsync(LocalWebAppHost host)
        => MainElement.LoadAsync(host);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    /// <param name="callback">The callback.</param>
    public Task LoadAsync(Task<LocalWebAppHost> host, Action<LocalWebAppHost> callback = null)
        => MainElement.LoadAsync(host, callback);

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version.</returns>
    public Task UpdateAsync(CancellationToken cancellationToken)
        => MainElement.UpdateAsync(cancellationToken);

    /// <summary>
    /// Sends a notification message to webpage.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="message">The message body to send.</param>
    public void Notify(string type, LocalWebAppNotificationMessage message)
        => MainElement.Notify(type, message);

    private void OnClosed(object sender, WindowEventArgs args)
    {
        if (backdrop != null) backdrop.Dispose();
        MainElement.Close();
    }

    private void OnTitleChanged(object sender, DataEventArgs<string> e)
        => Title = TitleElement.Text = e.Data;

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (backdrop == null) return;
        var isActive = args.WindowActivationState != WindowActivationState.Deactivated;
        backdrop.IsInputActive = isActive;
        TitleElement.Opacity = isActive ? 1 : 0.6;
    }

    private void OnCoreProcessFailed(LocalWebAppPage sender, CoreWebView2ProcessFailedEventArgs args)
        => CoreProcessFailed?.Invoke(this, args);

    private void OnCoreWebViewInitialized(LocalWebAppPage sender, CoreWebView2InitializedEventArgs args)
        => CoreWebView2Initialized?.Invoke(this, args);

    private void OnNavigationCompleted(LocalWebAppPage sender, CoreWebView2NavigationCompletedEventArgs args)
        => NavigationCompleted?.Invoke(this, args);

    private void OnNavigationStarting(LocalWebAppPage sender, CoreWebView2NavigationStartingEventArgs args)
        => NavigationStarting?.Invoke(this, args);

    private void OnNewWindowRequested(LocalWebAppPage sender, CoreWebView2NewWindowRequestedEventArgs args)
        => NewWindowRequested?.Invoke(this, args);

    private void OnWindowCloseRequested(LocalWebAppPage sender, object args)
    {
        _ = OnWindowCloseRequestedAsync();
        WindowCloseRequested?.Invoke(this, args);
    }

    private async Task OnWindowCloseRequestedAsync()
    {
        await Task.Delay(600);
        Close();
    }

    private void OnDownloadStarting(LocalWebAppPage sender, CoreWebView2DownloadStartingEventArgs args)
        => DownloadStarting?.Invoke(this, args);
}
