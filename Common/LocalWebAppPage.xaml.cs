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

namespace Trivial.UI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LocalWebAppPage : Page
{
    private LocalWebAppHost host;
    private readonly Dictionary<string, ILocalWebAppCommandHandler> proc = new();
    private readonly LocalWebAppBrowserMessageHandler messageHandler;
    private TabbedWebViewWindow tabbedWebViewWindowInstance;
    private bool isDevEnv;
    private Action continueHandler;

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
    /// <param name="window">The host window.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The async task.</returns>
    public async Task LoadDevPackageAsync(Window window, CancellationToken cancellationToken = default)
    {
        var dir = await VisualUtility.SelectFolderAsync(window);
        await LoadDevPackageAsync(dir, cancellationToken);
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
        InfoViewContainer.Visibility = Visibility.Visible;
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
        InfoViewContainer.Visibility = Visibility.Collapsed;
        if (host == null)
        {
            OnLoadError(new ArgumentNullException(nameof(host)));
            return;
        }

        Browser.Visibility = Visibility.Collapsed;
        var dir = host.ResourcePackageDirectory;
        InfoView.Model = host.Manifest;
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
                InfoViewContainer.Visibility = Visibility.Visible;
                return;
            }

            InfoViewContainer.Visibility = Visibility.Visible;
            continueHandler = () =>
            {
                ProgressElement.IsActive = true;
                Browser.Visibility = Visibility.Visible;
                Browser.CoreWebView2.Navigate(host.GetVirtualPath(homepage));
            };
            return;
        }

        if (showInfo)
        {
            ShowTitle(host);
            ProgressElement.IsActive = false;
            InfoViewContainer.Visibility = Visibility.Visible;
            continueHandler = () =>
            {
                ProgressElement.IsActive = true;
                Browser.Visibility = Visibility.Visible;
                Browser.CoreWebView2.Navigate(host.GetVirtualPath(homepage));
            };
            return;
        }

        Browser.Visibility = Visibility.Visible;
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
    /// Focuses the browser.
    /// </summary>
    /// <param name="value">The focus state.</param>
    public void FocusBrowser(FocusState value)
        => Browser.Focus(value);

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

    private void ShowTitle(LocalWebAppHost host)
    {
        var title = host.Manifest?.DisplayName?.Trim();
        if (string.IsNullOrEmpty(title)) return;
        TitleChanged?.Invoke(this, new(host.Manifest?.DisplayName));
    }

    private void OnDocumentTitleChanged(CoreWebView2 sender, object args)
        => TitleChanged?.Invoke(this, new DataEventArgs<string>(sender.DocumentTitle));

    private void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {
        var settings = Browser.CoreWebView2.Settings;
        var isDebug = IsDevEnvironmentEnabled;
        settings.AreDevToolsEnabled = isDebug;
        settings.AreDefaultContextMenusEnabled = false;
        sender.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
        sender.CoreWebView2.DownloadStarting += OnDownloadStarting;
        sender.CoreWebView2.FrameCreated += OnFrameCreated;
        sender.CoreWebView2.FrameNavigationStarting += OnFrameNavigationStarting;
        sender.CoreWebView2.FrameNavigationCompleted += OnFrameNavigationCompleted;
        sender.CoreWebView2.HistoryChanged += OnHistoryChanged;
        sender.CoreWebView2.ContainsFullScreenElementChanged += OnContainsFullScreenElementChanged;
        sender.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        sender.CoreWebView2.WindowCloseRequested += OnWindowCloseRequested;
        sender.CoreWebView2.PermissionRequested += OnPermissionRequested;
        CoreWebView2Initialized?.Invoke(this, args);
        var sb = new StringBuilder();
        sb.Append(@"(function () { if (window.localWebApp) return;
let postMsg = window.chrome && window.chrome.webview && typeof window.chrome.webview.postMessage === 'function' ? function (data) { window.chrome.webview.postMessage(data); } : function (data) { };
let hs = []; let stepNumber = 0;
function genRandomStr() {
  if (stepNumber >= Number.MAX_SAFE_INTEGER) stepNumber = 0; stepNumber++;
  return 'r' + Math.floor(Math.random() * 46655).toString(36) + stepNumber.toString(36) + (new Date().getTime().toString(36));
}
function sendRequest(handlerId, cmd, data, info, context, noResp, ref) {
  let req = { handler: handlerId, cmd, data, info, context, date: new Date(), trace: genRandomStr() }; let promise = null;
  if (!noResp) {
    promise = new Promise(function (resolve, reject) {
      let handler = {};
      handler.proc = function (ev) {
        if (!ev || !ev.data || ev.data.trace != req.trace) return;
        handler.invalid = true;
        if (context) ev.context = context;
        if (ev.data.error) reject(ev.data);
        else resolve(ev.data);
        try { if (ref) ref.response = ev.data; } catch (ex) { }
    }; hs.push({ h: handler, type: null });
  }); }
  postMsg(req); try { if (ref) ref.trace = req.trace; } catch (ex) { }
  return promise;
}
if (postMsg && typeof window.chrome.webview.addEventListener === 'function') {
  try {
    window.chrome.webview.addEventListener('message', function (ev) {
      let removing = [];
      for (let i in hs) {
        let source = hs[i] ?? {}; if (!ev || !ev.data || source.type != ev.data.type) continue;
        let item = source.h; if (!item) continue;
        if (typeof item.proc === 'function') {
          if (item.invalid != null) {
            let toRemove = false;
            if (item.invalid === true) {
              toRemove = true;
            } else if (typeof item.invalid === 'function') {
              if (item.invalid(ev)) toRemove = true;
            } else if (typeof item.invalid === 'number') {
              if (item.invalid <= 0) toRemove = true;
              else item.invalid--;
            }
            if (toRemove && !item.keep) {
              removing.push(source); continue;
            }
          }
          item.proc(ev); continue;
        }
        if (typeof item === 'function') item(ev);
      }
      for (let i in removing) {
        let item = removing[i]; if (!item) continue;
        let j = hs.indexOf(removing[i]);
        if (j >= 0) hs.splice(j, 1);
        if (typeof item.dispose === 'function') item.dispose();
      }
    });
  } catch (ex) { }
}
window.localWebApp = { 
  onMessage(type, callback) {
    if (!callback) return;
    if (typeof callback !== 'function' && typeof callback.proc !== 'function') return;
    hs.push({ h: callback, type });
  },
  getHandler(id) {
    if (!id || typeof id !== 'string') return null;
    return {
      id() {
        return id;
      },
      call(cmd, data, context, info, ref) {
        sendRequest(id, cmd, data, info, context, false, ref)
      },
      request(cmd, data, context, info) {
        return sendRequest(id, cmd, data, info, context, true, ref)
      }
    };
  },
  getCookie: function (key) {
    if (!key) return document.cookie;
    key = key + '='; let ca = document.cookie.split(';');
    for (let i in ca) {
      let c = ca[i]; while (c.charAt(0) == ' ') { c = c.substring(1); } if (c.startsWith(key)) return c.substring(key.length);
    }
    return '';
  },
  files: {
    list(dir, options) {
      if (!options) options = {};
      else if (typeof options === 'string') options = { q: options };
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'list-file', { path: dir, q: options.q, showHidden: options.showHidden }, null, options.context, false, options.ref);
    },
    listDrives(options) {
      if (options === true) options = { fixed: true };
      else if (options === false) options = { fixed: false };
      else if (!options) options = {};
      return sendRequest(null, 'list-drives', { fixed: options.fixed }, null, options.context, false, options.ref);
    },
    get(path, options) {
      if (!options) options = {};
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'get-file', { path, read: options.read, maxLength: options.maxLength }, null, options.context, false, options.ref);
    },
    write(path, value, options) {
      if (!options) options = {};
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'write-file', { path, value }, null, options.context, false, options.ref);
    },
    move(path, dest, options) {
      if (options === true) options = { override: true };
      else if (options === false) options = { override: false };
      else if (!options) options = {};
      if (!dest) dest = '';
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'move-file', { path, dest, override: options.override, dir: options.dir, copy: false }, null, options.context, false, options.ref);
    },
    copy(path, dest, options) {
      if (options === true) options = { override: true };
      else if (options === false) options = { override: false };
      else if (!options) options = {};
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'move-file', { path, dest, override: options.override, dir: options.dir, copy: true }, null, options.context, false, options.ref);
    },
    delete(path, options) {
      if (!options) options = {};
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'move-file', { path, dir: options.dir, copy: false }, null, options.context, false, options.ref);
    },
    md(path, options) {
      if (!options) options = {};
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'make-dir', { path }, null, options.context, false, options.ref);
    },
    open(path, options) {
      if (!options) options = {};
      else if (typeof options === 'string') options = { args: options }
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'open', { path, args: options.args, type: options.type }, null, options.context, false, options.ref);
    },
    listDownload(options) {
      if (options === true) options = { open: true };
      else if (options === false) options = { open: false };
      else if (typeof options === 'number') options = { max: options };
      else if (!options) options = {};
      return sendRequest(null, 'download-list', { open: options.open, max: options.max }, null, options.context, false, options.ref);
    }
  },
  cryptography: {
    encrypt(alg, value, key, iv, options) {
      if (!options) options = {};
      return sendRequest(null, 'symmetric', { value, alg, key, iv, decrypt: false }, null, options.context, false, options.ref);
    },
    decrypt(alg, value, key, iv, options) {
      if (!options) options = {};
      return sendRequest(null, 'symmetric', { value, alg, key, iv, decrypt: true }, null, options.context, false, options.ref);
    },
    verify(alg, value, key, test, options) {
      if (!options) options = {};
      return sendRequest(null, 'verify', { value, alg, key, test, type: options.type }, null, options.context, false, options.ref);
    },
    sign(alg, value, key, options) {
      if (!options) options = {};
      return sendRequest(null, 'sign', { value, alg, key, type: options.type }, null, options.context, false, options.ref);
    },
    hash(alg, value, options) {
      if (!options) options = {};
      else if (typeof options === 'string') options = { test: options }
      return sendRequest(null, 'hash', { value, alg, test: options.test, type: options.type }, null, options.context, false, options.ref);
    }
  },
  text: {
    encodeBase64(s, url) {
      if (!s) return s; if (typeof s !== 'string') return null;
      s = window.btoa(s); return url ? s.replace(/\+/g, '-').replace(/\//g, '_').replace(/\=/g, '') : s;
    },
    decodeBase64(s, url) {
      if (!s) return s; if (typeof s !== 'string') return null;
      if (url) s = s.replace(/\-/g, '+').replace(/_/g, '/').replace(/\=/g, '');
      s = window.atob(s); return s;
    },
    encodeUri(s, parameter) {
      return s ? (parameter ? encodeURIComponent(s) : encodeURI(s)) : s;
    },
    decodeUri(s, parameter) {
      return s ? (parameter ? decodeURIComponent(s) : decodeURI(s)) : s;
    }
  },
  hostApp: {
    theme(options) {
      if (!options) options = {};
      return sendRequest(null, 'theme', {}, null, options.context, false, options.ref);
    },
    checkUpdate(options) {
      if (options === true) options = { check: true };
      else if (options === false) options = { check: false };
      else if (!options) options = {};
      return sendRequest(null, 'check-update', { check: options.check }, null, options.context, false, options.ref);
    },
    window(value) {
      if (!value) value = {};
      else if (typeof value === 'string') value = { state: value };
      return sendRequest(null, 'window', { state: value.state, width: value.width || value.w, height: value.height || value.h, top: value.top || value.y, left: value.left || value.x, focus: value.focus, physical: value.physical }, null, value.context, false, options.ref);
    },
    handlers(options) {
      if (!options) options = {};
      return sendRequest(null, 'handlers', {}, null, options.context, false, options.ref);
    }
  },
  hostInfo: ");
        sb.Append(LocalWebAppExtensions.GetEnvironmentInformation(host.Manifest).ToString(IndentStyles.Compact));
        sb.Append(", dataRes: ");
        sb.Append(host.DataResources.ToString(IndentStyles.Compact));
        sb.Append(", strRes: ");
        var resourceReg = new JsonObjectNode();
        resourceReg.SetRange(host.DataStrings);
        sb.Append(resourceReg.ToString(IndentStyles.Compact));
        sb.AppendLine(" }; })();");
        _ = Browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(sb.ToString());
    }

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

    private async Task OnNewWindowRequestedAsync(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        var uri = VisualUtility.TryCreateUri(args.Uri);
        if (uri == null) return;
        args.Handled = true;
        var deferral = args.GetDeferral();
        var window = tabbedWebViewWindowInstance;
        if (tabbedWebViewWindowInstance == null)
        {
            window = new TabbedWebViewWindow
            {
                IsReadOnly = true,
                DownloadList = DownloadList
            };
            OnWindowCreate?.Invoke(window);
        }

        var webview = window.Add(uri);
        await webview.EnsureCoreWebView2Async();
        args.NewWindow = webview.CoreWebView2;
        OnWebViewTabInitialized(webview.CoreWebView2);
        deferral.Complete();
        window.TabView.WebViewTabCreated += (sender2, args2) =>
        {
            _ = OnWebViewTabInitializedAsync(window, args2.WebView);
        };
        window.Closed += (sender2, args2) =>
        {
            tabbedWebViewWindowInstance = null;
        };
        try
        {
            window.TabView.RequestedTheme = Browser.RequestedTheme;
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

        tabbedWebViewWindowInstance = window;
        window.Activate();
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

    private void OnActualThemeChanged(FrameworkElement sender, object args)
        => Notify("themeChanged", new(messageHandler?.GetTheme(), "system"));

    private void OnCloseInfoButton(object sender, RoutedEventArgs e)
    {
        InfoViewContainer.Visibility = Visibility.Collapsed;
        InfoView.Model = null;
        var h = continueHandler;
        continueHandler = null;
        h?.Invoke();
    }
}
