using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Text;
using Trivial.Web;

namespace Trivial.UI;

/// <summary>
/// The core browser client of local web app.
/// </summary>
public class LocalWebAppCoreClient
{
    private LocalWebAppHost host;
    private readonly Dictionary<string, ILocalWebAppCommandHandler> proc = new();
    private readonly LocalWebAppBrowserMessageHandler messageHandler;
    private bool isDevEnv;

    /// <summary>
    /// Initializes a new instance of the LocalWebAppCoreClient class.
    /// </summary>
    /// <param name="browser">The browser instance.</param>
    public LocalWebAppCoreClient(WebView2 browser)
    {
        if (browser is null) return;
        Browser = browser;
        messageHandler = new(Browser);
        _ = OnInitAsync();
    }

    private async Task OnInitAsync()
    {
        await Browser.EnsureCoreWebView2Async();
        Browser.WebMessageReceived += OnWebMessageReceived;
        Browser.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
    }

    /// <summary>
    /// Adds or removes an event occured on load failed.
    /// </summary>
    public event DataEventHandler<Exception> LoadFailed;

    /// <summary>
    /// Adds or removes an event occured on load succeeded.
    /// </summary>
    public event DataEventHandler<string> Loaded;

    /// <summary>
    /// Gets or sets the alert handler during load the package which is not trusted.
    /// </summary>
    public Action<Action> Alert { get; set; }

    /// <summary>
    /// Gets the identifier of the resource package.
    /// </summary>
    public string ResourcePackageId => host?.Manifest?.Id ?? host?.ResourcePackageId;

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
    /// Gets or sets the Tabbed web view window instance.
    /// </summary>
    public TabbedWebViewWindow TabbedWindow;

    /// <summary>
    /// Gets the browser.
    /// </summary>
    public WebView2 Browser { get; }

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
        Alert?.Invoke(() => _ = LoadAsync(host));
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
        if (host == null)
        {
            OnLoadError(new ArgumentNullException(nameof(host)));
            return;
        }

        var dir = host.ResourcePackageDirectory;
        if (dir == null || !dir.Exists)
        {
            OnLoadError(new DirectoryNotFoundException("The app directory is not found."));
            return;
        }

        this.host = host;
        try
        {
            await Browser.EnsureCoreWebView2Async();
            Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(host.VirtualHost, dir.FullName, CoreWebView2HostResourceAccessKind.Allow);
        }
        catch (Exception ex)
        {
            LoadFailed?.Invoke(this, new(ex));
            throw;
        }

        var homepage = host.Manifest.HomepagePath?.Trim();
        if (string.IsNullOrEmpty(homepage)) homepage = "index.html";
        if (!host.IsVerified)
        {
            if (!IsDevEnvironmentEnabled)
            {
                OnLoad(homepage);
                return;
            }

            Alert?.Invoke(() =>
            {
                OnLoad(homepage);
                NavigateHomepage(homepage);
            });
            return;
        }

        if (showInfo)
        {
            Alert?.Invoke(() =>
            {
                OnLoad(homepage);
                NavigateHomepage(homepage);
            });
            return;
        }

        OnLoad(homepage);
        NavigateHomepage(homepage);
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

    private void NavigateHomepage(string homepage)
    {
        try
        {
            Browser.CoreWebView2.Navigate(host.GetVirtualPath(homepage));
        }
        catch (Exception ex)
        {
            LoadFailed?.Invoke(this, new(ex));
            throw;
        }
    }

    private void OnDownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        DownloadList.Add(args.DownloadOperation);
    }

    private void OnNewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        var window = TabbedWindow;
        if (window is null) return;
        _ = LocalWebAppPage.OnNewWindowRequestedAsync(args, window, OnWebViewTabInitialized, Browser.RequestedTheme);
        window.Activate();
    }

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
    }

    private async Task OnWebMessageReceivedAsync(WebView2 sender, JsonObjectNode json)
    {
        if (json == null) return;
        json = await LocalWebAppExtensions.OnWebMessageReceivedAsync(host, json, sender.Source, proc, WindowController, messageHandler);
        sender.CoreWebView2.PostWebMessageAsJson(json.ToString());
    }

    private void OnWebViewTabInitialized(CoreWebView2 webview)
    {
        webview.DownloadStarting += OnDownloadStarting;
    }

    private void OnLoad(string homepage)
    {
        LocalWebAppPage.AppendEnvironmentInfo(Browser, host, isDevEnv);
        Loaded?.Invoke(this, new DataEventArgs<string>(homepage));
    }

    private void OnLoadError(Exception ex)
    {
        if (ex == null) return;
        LoadFailed?.Invoke(this, new DataEventArgs<Exception>(ex));
    }
}
