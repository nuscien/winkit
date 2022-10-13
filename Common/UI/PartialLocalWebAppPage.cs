using Microsoft.UI.Xaml.Controls;
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

namespace Trivial.UI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LocalWebAppPage
{
    private LocalWebAppHost host;
    private readonly Dictionary<string, ILocalWebAppCommandHandler> proc = new();
    private readonly LocalWebAppBrowserMessageHandler messageHandler;
    private bool isDevEnv;
    private Action continueHandler;

    /// <summary>
    /// Occurs when the web view core processes failed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2ProcessFailedEventArgs> CoreProcessFailed;

    /// <summary>
    /// Occurs when the web view core has initialized.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2InitializedEventArgs> CoreWebView2Initialized;

    /// <summary>
    /// Occurs when the navigation is completed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;

    /// <summary>
    /// Occurs when the navigation is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NavigationStartingEventArgs> NavigationStarting;

    /// <summary>
    /// Occurs when the web message core has received.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2WebMessageReceivedEventArgs> WebMessageReceived;

    /// <summary>
    /// Occurs when the new window request sent.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested;

    /// <summary>
    /// Occurs when the webpage send request to close itself.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, object> WindowCloseRequested;

    /// <summary>
    /// Occurs when the downloading is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2DownloadStartingEventArgs> DownloadStarting;

    /// <summary>
    /// Occurs when the downloading is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2PermissionRequestedEventArgs> PermissionRequested;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is created.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2FrameCreatedEventArgs> FrameCreated;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is completed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NavigationCompletedEventArgs> FrameNavigationCompleted;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NavigationStartingEventArgs> FrameNavigationStarting;

    /// <summary>
    /// Occurs when the history has changed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, object> HistoryChanged;

    /// <summary>
    /// Occurs when fullscreen request sent including to enable and disable.
    /// </summary>
    public event DataEventHandler<bool> ContainsFullScreenElementChanged;

    /// <summary>
    /// Adds or removes an event occured on title changed.
    /// </summary>
    public event DataEventHandler<string> TitleChanged;

    /// <summary>
    /// Adds or removes an event occured on load failed.
    /// </summary>
    public event DataEventHandler<Exception> LoadFailed;

    /// <summary>
    /// Gets the identifier of the resource package.
    /// </summary>
    public string ResourcePackageId => host?.Manifest?.Id;

    /// <summary>
    /// Gets the display name of the resource package.
    /// </summary>
    public string ResourcePackageDisplayName => host?.Manifest?.DisplayName;

    /// <summary>
    /// Gets the version string of the resource package.
    /// </summary>
    public string ResourcePackageVersion => host?.Manifest?.Version;

    /// <summary>
    /// Gets the copyright string of the resource package.
    /// </summary>
    public string ResourcePackageCopyright => host?.Manifest?.Copyright;

    /// <summary>
    /// Gets the publisher name of the resource package.
    /// </summary>
    public string ResourcePackagePublisherName => host?.Manifest?.PublisherName;

    /// <summary>
    /// Gets the description of the resource package.
    /// </summary>
    public string ResourcePackageDescription => host?.Manifest?.Description;

    /// <summary>
    /// Gets the relative icon path of the resource package.
    /// </summary>
    public string ResourcePackageIcon => host?.Manifest?.Icon;

    /// <summary>
    /// Gets the website URL of the resource package.
    /// </summary>
    public string ResourcePackageWebsite => host?.Manifest?.Website;

    /// <summary>
    /// Gets the download list.
    /// </summary>
    public List<CoreWebView2DownloadOperation> DownloadList
    {
        get => messageHandler.DownloadList;
        internal set => messageHandler.DownloadList = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether it is in debug mode to ignore any signature verification and enable Microsoft Edge DevTools.
    /// </summary>
    public bool IsDevEnvironmentEnabled
    {
        get
        {
            return isDevEnv;
        }

        set
        {
            isDevEnv = value;
            try
            {
                if (Browser.CoreWebView2?.Settings != null) Browser.CoreWebView2.Settings.AreDevToolsEnabled = value;
            }
            catch (NullReferenceException)
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
        }
    }

    /// <summary>
    /// Gets the available new version to update.
    /// </summary>
    public string NewVersionAvailable => host?.NewVersionAvailable;

    /// <summary>
    /// Gets or sets a value indicating whether disable to create a default tabbed browser for new window request.
    /// </summary>
    public bool DisableNewWindowRequestHandling { get; set; }

    /// <summary>
    /// Gets or sets the handler occuring on the default browser window is created.
    /// </summary>
    public Action<TabbedWebViewWindow> OnWindowCreate { get; set; }

    /// <summary>
    /// Gets the options.
    /// </summary>
    public LocalWebAppOptions Options => host?.Options;

    /// <summary>
    /// Gets a value indicating whether the app is verified.
    /// </summary>
    public bool IsVerified => host?.IsVerified ?? false;

    /// <summary>
    /// Gets or sets the window controller.
    /// </summary>
    public IBasicWindowStateController WindowController { get; set; }

    /// <summary>
    /// Gets the a value indicating whether contains the full screen element.
    /// </summary>
    public bool ContainsFullScreenElement => Browser.CoreWebView2?.ContainsFullScreenElement ?? false;

    /// <summary>
    /// Gets a value indicating whether the browser can go back.
    /// </summary>
    public bool CanGoBack => Browser.CanGoBack;

    /// <summary>
    /// Gets a value indicating whether the browser can go forward.
    /// </summary>
    public bool CanGoForward => Browser.CanGoForward;

    /// <summary>
    /// Goes back.
    /// </summary>
    public void GoBack()
        => Browser.GoBack();

    /// <summary>
    /// Goes back.
    /// </summary>
    public void GoForward()
        => Browser.GoForward();

    /// <summary>
    /// Goes back.
    /// </summary>
    public void ReloadPage()
        => Browser.Reload();

    /// <summary>
    /// Gets absolute icon path of resource package.
    /// </summary>
    public string GetResourcePackageIconPath()
        => host?.GetResourcePackageIconPath();

    /// <summary>
    /// Tests if the host loaded is the specific one.
    /// </summary>
    /// <param name="host">The host to test.</param>
    /// <returns>true if they are the same one; otherwise, false.</returns>
    public bool IsHost(LocalWebAppHost host)
        => host == this.host;

    /// <summary>
    /// Executes JavaScript.
    /// </summary>
    /// <param name="javascriptCode">The code to execute.</param>
    /// <returns>Value returned.</returns>
    public async Task<string> ExecuteScriptAsync(string javascriptCode)
    {
        if (string.IsNullOrWhiteSpace(javascriptCode)) return null;
        await Browser.EnsureCoreWebView2Async();
        return await Browser.ExecuteScriptAsync(javascriptCode);
    }

    /// <summary>
    /// Load a dev local web app.
    /// </summary>
    /// <param name="dir">The root directory.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The async task.</returns>
    public async Task LoadDevPackageAsync(DirectoryInfo dir, CancellationToken cancellationToken = default)
    {
        if (dir == null || !dir.Exists)
        {
            var ex = new DirectoryNotFoundException("The directory is not found.");
            OnLoadError(ex);
            throw ex;
        }

        LocalWebAppHost host;
        try
        {
            host = await LocalWebAppHost.LoadDevPackageAsync(dir, cancellationToken);
        }
        catch (Exception ex)
        {
            OnLoadError(ex);
            throw;
        }

        if (host == null)
        {
            OnLoadError(new InvalidOperationException("Failed to load app."));
            return;
        }

        IsDevEnvironmentEnabled = true;
        ShowTitle(host);
        SetInfoViewContainerVisibility(true);
        ProgressElement.IsActive = false;
        continueHandler = () => _ = LoadAsync(host);
    }

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="resourcePackageId">The identifier of the resource package.</param>
    public async Task LoadAsync(string resourcePackageId)
    {
        if (resourcePackageId == null)
        {
            OnLoadError(new ArgumentNullException(nameof(resourcePackageId)));
            return;
        }

        LocalWebAppHost host;
        try
        {
            host = await LocalWebAppHost.LoadAsync(resourcePackageId, IsDevEnvironmentEnabled);
        }
        catch (Exception ex)
        {
            OnLoadError(ex);
            throw;
        }

        if (host == null)
            OnLoadError(new InvalidOperationException("Failed to load app."));
        else
            await LoadAsync(host);
    }

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="options">The options of the standalone local web app.</param>
    public async Task LoadAsync(LocalWebAppOptions options)
    {
        if (options == null)
        {
            OnLoadError(new ArgumentNullException(nameof(options)));
            return;
        }

        LocalWebAppHost host;
        try
        {
            host = await LocalWebAppHost.LoadAsync(options, IsDevEnvironmentEnabled);
        }
        catch (Exception ex)
        {
            OnLoadError(ex);
            throw;
        }

        if (host == null)
            OnLoadError(new InvalidOperationException("Failed to load app."));
        else
            await LoadAsync(host);
    }

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="hostTask">The standalone local web app host.</param>
    /// <param name="showInfo">true if show the information before loading; otherwise, false.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="error">The error handling.</param>
    public async Task LoadAsync(Task<LocalWebAppHost> hostTask, bool showInfo, Action<LocalWebAppHost> callback = null, Action<Exception> error = null)
    {
        if (hostTask == null)
        {
            OnLoadError(new ArgumentNullException(nameof(hostTask)));
            return;
        }

        LocalWebAppHost host;
        try
        {
            host = await hostTask;
        }
        catch (Exception ex)
        {
            OnLoadError(ex);
            error?.Invoke(ex);
            throw;
        }

        if (host == null)
        {
            var ex = new InvalidOperationException("Failed to load app.");
            OnLoadError(ex);
            error?.Invoke(ex);
        }
        else
        {
            await LoadAsync(host, showInfo);
            callback?.Invoke(host);
        }
    }

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="hostTask">The standalone local web app host.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="error">The error handling.</param>
    public Task LoadAsync(Task<LocalWebAppHost> hostTask, Action<LocalWebAppHost> callback = null, Action<Exception> error = null)
        => LoadAsync(hostTask, false, callback, error);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone local web app host.</param>
    /// <param name="showInfo">true if show the information before loading; otherwise, false.</param>
    public async Task LoadAsync(LocalWebAppHost host, bool showInfo)
    {
        //Browser.NavigateToString(@"<html><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" ><base target=""_blank"" /></head><body></body></html>");
        SetInfoViewContainerVisibility(false);
        if (host == null)
        {
            OnLoadError(new ArgumentNullException(nameof(host)));
            return;
        }

        SetBrowserVisibility(false);
        var dir = host.ResourcePackageDirectory;
        InfoView.Model = host.Manifest;
        var icon = host.GetResourcePackageIconPath();
        if (!string.IsNullOrWhiteSpace(icon)) InfoView.Icon = icon;
        if (dir == null || !dir.Exists)
        {
            OnLoadError(new DirectoryNotFoundException("The app directory is not found."));
            return;
        }

        this.host = host;
        await Browser.EnsureCoreWebView2Async();
        Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(host.VirtualHost, dir.FullName, CoreWebView2HostResourceAccessKind.Allow);
        var homepage = host.Manifest.HomepagePath?.Trim();
        if (string.IsNullOrEmpty(homepage)) homepage = "index.html";
        ProgressElement.IsActive = true;
        if (!host.IsVerified)
        {
            ShowTitle(host);
            NotificationBar.Title = string.IsNullOrWhiteSpace(LocalWebAppSettings.CustomizedLocaleStrings.ErrorTitle) ? "Error" : LocalWebAppSettings.CustomizedLocaleStrings.ErrorTitle;
            NotificationBar.Message = string.IsNullOrWhiteSpace(LocalWebAppSettings.CustomizedLocaleStrings.InvalidFileSignature) ? "Invalid file signatures." : LocalWebAppSettings.CustomizedLocaleStrings.InvalidFileSignature;
            NotificationBar.Severity = InfoBarSeverity.Error;
            NotificationBar.IsOpen = true;
            ProgressElement.IsActive = false;
            MonitorSingleton?.OnErrorNotification(this, NotificationBar, new SecurityException("Invalid file signatures."));
            if (!IsDevEnvironmentEnabled)
            {
                SetInfoViewContainerVisibility(true);
                return;
            }

            SetInfoViewContainerVisibility(true);
            continueHandler = () =>
            {
                ProgressElement.IsActive = true;
                SetBrowserVisibility(true);
                Browser.CoreWebView2.Navigate(host.GetVirtualPath(homepage));
            };
            return;
        }

        if (showInfo)
        {
            ShowTitle(host);
            ProgressElement.IsActive = false;
            SetInfoViewContainerVisibility(true);
            continueHandler = () =>
            {
                ProgressElement.IsActive = true;
                SetBrowserVisibility(true);
                Browser.CoreWebView2.Navigate(host.GetVirtualPath(homepage));
            };
            return;
        }

        SetBrowserVisibility(true);
        Browser.CoreWebView2.Navigate(host.GetVirtualPath(homepage));
    }

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone local web app host.</param>
    public Task LoadAsync(LocalWebAppHost host)
        => LoadAsync(host, false);

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version.</returns>
    public Task<string> UpdateAsync(CancellationToken cancellationToken)
    {
        if (host == null) return Task.FromResult<string>(null);
        return host.UpdateAsync(cancellationToken);
    }

    /// <summary>
    /// Sends a notification message to webpage.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="message">The message body to send.</param>
    public void Notify(string type, LocalWebAppNotificationMessage message)
    {
        type = type?.Trim();
        if (string.IsNullOrEmpty(type) || message == null || Browser.CoreWebView2 == null) return;
        var json = new JsonObjectNode
        {
            { "type", type },
            { "data", message.Data ?? new() },
            { "info", message.AdditionalInfo ?? new() },
            { "source", message.Source },
            { "message", message.Message },
            { "sent", DateTime.Now },
            { "id", Guid.NewGuid() }
        };
        Browser.CoreWebView2.PostWebMessageAsJson(json.ToString());
    }

    /// <summary>
    /// Stops navigating.
    /// </summary>
    public void Close()
    {
        var webview2 = Browser.CoreWebView2;
        if (webview2 == null) return;
        Browser.Close();
    }

    /// <summary>
    /// Tests if there is the command handler of given identifier.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <returns>true if exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">id was null.</exception>
    public bool ContainsCommandHandler(string id)
        => proc.ContainsKey(id);

    /// <summary>
    /// Tries to get the specific command handler.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <param name="handler">The command handler.</param>
    /// <returns>true if exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">id was null.</exception>
    public bool TryGetCommandHandler(string id, out ILocalWebAppCommandHandler handler)
        => proc.TryGetValue(id, out handler);

    /// <summary>
    /// Tries to get the specific command handler.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <returns>The command handler..</returns>
    /// <exception cref="ArgumentNullException">id was null.</exception>
    public ILocalWebAppCommandHandler TryGetCommandHandler(string id)
        => proc.TryGetValue(id, out var callback) ? callback : null;

    /// <summary>
    /// Registers a command handler. It will override the existed one.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <param name="handler">The command handler.</param>
    /// <exception cref="ArgumentNullException">id was null.</exception>
    public void RegisterCommandHandler(string id, ILocalWebAppCommandHandler handler)
        => proc[id] = handler;

    /// <summary>
    /// Removes the command handler.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <returns>true if remove succeeded; otherwise, false.</returns>
    public bool RemoveCommandHandler(string id)
        => proc.Remove(id);

    /// <summary>
    /// Maps a folder as a virtual host name.
    /// </summary>
    /// <param name="hostName">The virtual host name.</param>
    /// <param name="folderPath">The folder path to map.</param>
    /// <param name="accessKind">The access kind.</param>
    public void SetVirtualHostNameToFolderMapping(string hostName, string folderPath, CoreWebView2HostResourceAccessKind accessKind)
        => Browser.CoreWebView2?.SetVirtualHostNameToFolderMapping(hostName, folderPath, accessKind);

    /// <summary>
    /// Clears the mapping of the specific virtual host name.
    /// </summary>
    /// <param name="hostName">The virtual host name.</param>
    public void ClearVirtualHostNameToFolderMapping(string hostName)
        => Browser.CoreWebView2?.ClearVirtualHostNameToFolderMapping(hostName);

    /// <summary>
    /// Prints to PDF format file.
    /// </summary>
    /// <param name="path">The output path of the PDF format file.</param>
    /// <param name="printSettings">The print settings.</param>
    /// <returns>true if print succeeded; otherwise, false.</returns>
    public async Task<bool> PrintToPdfAsync(string path, CoreWebView2PrintSettings printSettings)
    {
        await Browser.EnsureCoreWebView2Async();
        return await Browser.CoreWebView2.PrintToPdfAsync(path, printSettings);
    }

    /// <summary>
    /// Opens the default download dialog.
    /// </summary>
    public void OpenDefaultDownloadDialog()
        => Browser.CoreWebView2?.OpenDefaultDownloadDialog();

    /// <summary>
    /// Closes the default download dialog.
    /// </summary>
    public void CloseDefaultDownloadDialog()
        => Browser.CoreWebView2?.CloseDefaultDownloadDialog();

    /// <summary>
    /// Gets the browser settings.
    /// </summary>
    /// <returns>The browser settings.</returns>
    public async Task<CoreWebView2Settings> GetBrowserSettingsAsync()
    {
        await Browser.EnsureCoreWebView2Async();
        return Browser.CoreWebView2.Settings;
    }

    private void ShowTitle(LocalWebAppHost host)
    {
        var title = host.Manifest?.DisplayName?.Trim();
        if (string.IsNullOrEmpty(title)) return;
        TitleChanged?.Invoke(this, new(host.Manifest?.DisplayName));
    }

    private void OnDocumentTitleChanged(CoreWebView2 sender, object args)
        => TitleChanged?.Invoke(this, new DataEventArgs<string>(sender.DocumentTitle));

    private void OnDownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        DownloadList.Add(args.DownloadOperation);
        DownloadStarting?.Invoke(this, args);
    }

    private void OnPermissionRequested(CoreWebView2 sender, CoreWebView2PermissionRequestedEventArgs args)
        => PermissionRequested?.Invoke(this, args);

    private void OnContainsFullScreenElementChanged(CoreWebView2 sender, object args)
        => ContainsFullScreenElementChanged?.Invoke(this, new DataEventArgs<bool>(Browser.CoreWebView2.ContainsFullScreenElement));

    private void OnNewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        NewWindowRequested?.Invoke(this, args);
        if (DisableNewWindowRequestHandling) return;
        _ = OnNewWindowRequestedAsync(sender, args);
    }

    private void OnWindowCloseRequested(CoreWebView2 sender, object args)
        => WindowCloseRequested?.Invoke(this, args);

    private void OnFrameCreated(CoreWebView2 sender, CoreWebView2FrameCreatedEventArgs args)
        => FrameCreated?.Invoke(this, args);

    private void OnFrameNavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        => FrameNavigationStarting?.Invoke(this, args);

    private void OnFrameNavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        => FrameNavigationCompleted?.Invoke(this, args);

    private void OnHistoryChanged(CoreWebView2 sender, object args)
        => HistoryChanged?.Invoke(this, args);

    private void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        ProgressElement.IsActive = false;
        NavigationCompleted?.Invoke(this, args);
    }

    private void OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        => NavigationStarting?.Invoke(this, args);

    private void OnWebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        if (sender == null) return;
        JsonObjectNode json = null;
        try
        {
            var msg = args.WebMessageAsJson;
            json = JsonObjectNode.TryParse(msg);
        }
        catch (FormatException)
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

        _ = OnWebMessageReceivedAsync(sender, json);
        WebMessageReceived?.Invoke(this, args);
    }

    private async Task OnWebMessageReceivedAsync(WebView2 sender, JsonObjectNode json)
    {
        if (json == null) return;
        json = await LocalWebAppExtensions.OnWebMessageReceivedAsync(host, json, sender.Source, proc, WindowController, messageHandler);
        sender.CoreWebView2.PostWebMessageAsJson(json.ToString());
    }

    private void OnCoreProcessFailed(WebView2 sender, CoreWebView2ProcessFailedEventArgs args)
    {
        ProgressElement.IsActive = false;
        CoreProcessFailed?.Invoke(this, args);
    }

    /// <summary>
    /// Occurs on web view is initialized.
    /// </summary>
    /// <param name="window">The window.</param>
    /// <param name="webview">The web view.</param>
    private async Task OnWebViewTabInitializedAsync(TabbedWebViewWindow window, SingleWebView webview)
    {
        await webview.EnsureCoreWebView2Async();
        OnWebViewTabInitialized(webview.CoreWebView2);
    }

    private void OnWebViewTabInitialized(CoreWebView2 webview)
    {
        webview.DownloadStarting += OnDownloadStarting;
    }
}
