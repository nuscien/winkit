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
/// The local standalone local web app window.
/// </summary>
public sealed partial class LocalWebAppWindow : Window
{
    private readonly SystemBackdropClient backdrop;
    private double titleHeight = 28;

    /// <summary>
    /// Initializes a new instance of the LocalWebAppWindow class.
    /// </summary>
    public LocalWebAppWindow()
    {
        InitializeComponent();
        Title = TitleElement.Text = string.IsNullOrWhiteSpace(LocalWebAppSettings.CustomizedLocaleStrings.Loading) ? "Loading…" : LocalWebAppSettings.CustomizedLocaleStrings.Loading;
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
        MainElement.WindowController = new BasicWindowStateController(this);
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
    /// Gets or sets the title height.
    /// </summary>
    public double TitleHeight
    {
        get
        {
            return titleHeight;
        }

        set
        {
            titleHeight = value;
            TitleRow.Height = new(value);
        }
    }

    /// <summary>
    /// Gets the identifier of the resource package.
    /// </summary>
    public string ResourcePackageId => MainElement.ResourcePackageId;

    /// <summary>
    /// Gets the display name of the resource package.
    /// </summary>
    public string ResourcePackageDisplayName => MainElement.ResourcePackageDisplayName;

    /// <summary>
    /// Gets the version string of the resource package.
    /// </summary>
    public string ResourcePackageVersion => MainElement.ResourcePackageVersion;

    /// <summary>
    /// Gets the copyright string of the resource package.
    /// </summary>
    public string ResourcePackageCopyright => MainElement.ResourcePackageCopyright;

    /// <summary>
    /// Gets the publisher name of the resource package.
    /// </summary>
    public string ResourcePackagePublisherName => MainElement.ResourcePackagePublisherName;

    /// <summary>
    /// Gets the description of the resource package.
    /// </summary>
    public string ResourcePackageDescription => MainElement.ResourcePackageDescription;

    /// <summary>
    /// Gets the relative icon path of the resource package.
    /// </summary>
    public string ResourcePackageIcon => MainElement.ResourcePackageIcon;

    /// <summary>
    /// Gets the website URL of the resource package.
    /// </summary>
    public string ResourcePackageWebsite => MainElement.ResourcePackageWebsite;

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
    /// Gets the available new version to update.
    /// </summary>
    public string NewVersionAvailable => MainElement.NewVersionAvailable;

    /// <summary>
    /// Gets or sets a value indicating whether disable to create a default tabbed browser for new window request.
    /// </summary>
    public bool DisableNewWindowRequestHandling
    {
        get => MainElement.DisableNewWindowRequestHandling;
        set => MainElement.DisableNewWindowRequestHandling = value;
    }

    /// <summary>
    /// Gets or sets the default background color of the browser.
    /// </summary>
    public Windows.UI.Color BrowserBackgroundColor
    {
        get => MainElement.BrowserBackgroundColor;
        set => MainElement.BrowserBackgroundColor = value;
    }

    /// <summary>
    /// Gets the a value indicating whether contains the full screen element.
    /// </summary>
    public bool ContainsFullScreenElement => MainElement.ContainsFullScreenElement;

    /// <summary>
    /// Gets a value indicating whether the browser can go back.
    /// </summary>
    public bool CanGoBack => MainElement.CanGoBack;

    /// <summary>
    /// Gets a value indicating whether the browser can go forward.
    /// </summary>
    public bool CanGoForward => MainElement.CanGoForward;

    /// <summary>
    /// Gets or sets the image source of the icon.
    /// </summary>
    public ImageSource IconImageSource
    {
        get
        {
            return IconElement.Source;
        }

        set
        {
            TitleElement.Margin = new Thickness(value != null ? 28 : 10, 0, 32, 0);
            IconElement.Source = value;
        }
    }

    /// <summary>
    /// Gets or sets the handler occuring on the default browser window is created.
    /// </summary>
    public Action<TabbedWebViewWindow> OnWindowCreate
    {
        get => MainElement.OnWindowCreate;
        set => MainElement.OnWindowCreate = value;
    }

    /// <summary>
    /// Gets the options.
    /// </summary>
    public LocalWebAppOptions Options => MainElement.Options;

    /// <summary>
    /// Gets a value indicating whether the app is verified.
    /// </summary>
    public bool IsVerified => MainElement.IsVerified;

    /// <summary>
    /// Goes back.
    /// </summary>
    public void GoBack()
        => MainElement.GoBack();

    /// <summary>
    /// Goes back.
    /// </summary>
    public void GoForward()
        => MainElement.GoForward();

    /// <summary>
    /// Goes back.
    /// </summary>
    public void ReloadPage()
        => MainElement.ReloadPage();

    /// <summary>
    /// Executes JavaScript.
    /// </summary>
    /// <param name="javascriptCode">The code to execute.</param>
    /// <returns>Value returned.</returns>
    public Task<string> ExecuteScriptAsync(string javascriptCode)
        => MainElement.ExecuteScriptAsync(javascriptCode);

    /// <summary>
    /// Load a dev local web app.
    /// </summary>
    /// <returns>The async task.</returns>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    public Task LoadDevPackageAsync(CancellationToken cancellationToken = default)
        => MainElement.LoadDevPackageAsync(this, cancellationToken);

    /// <summary>
    /// Load a dev local web app.
    /// </summary>
    /// <param name="dir">The root directory.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    public Task LoadDevPackageAsync(DirectoryInfo dir, CancellationToken cancellationToken = default)
        => MainElement.LoadDevPackageAsync(dir, cancellationToken);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="resourcePackageId">The identifier of the resource package.</param>
    public Task LoadAsync(string resourcePackageId)
        => MainElement.LoadAsync(resourcePackageId);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="options">The options of the standalone local web app.</param>
    public Task LoadAsync(LocalWebAppOptions options)
        => MainElement.LoadAsync(options);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone local web app host.</param>
    public Task LoadAsync(LocalWebAppHost host)
        => MainElement.LoadAsync(host);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone local web app host.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="error">The error handling.</param>
    public Task LoadAsync(Task<LocalWebAppHost> host, Action<LocalWebAppHost> callback = null, Action<Exception> error = null)
        => MainElement.LoadAsync(host, callback, error);

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
    /// <param name="handler">The sender.</param>
    /// <param name="type">The message type.</param>
    /// <param name="message">The message body to send.</param>
    public void Notify(ILocalWebAppCommandHandler handler, string type, LocalWebAppNotificationMessage message)
        => MainElement.Notify(handler, type, message);

    /// <summary>
    /// Stops progress ring.
    /// </summary>
    public void StopLoading()
        => MainElement.StopLoading();

    /// <summary>
    /// Shows notification.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="message">The message.</param>
    /// <param name="severity">The severity.</param>
    public void ShowNotification(string title, string message, InfoBarSeverity severity)
        => MainElement.ShowNotification(title, message, severity);

    /// <summary>
    /// Focuses the browser.
    /// </summary>
    /// <param name="value">The focus state.</param>
    public void FocusBrowser(FocusState value)
        => MainElement.Focus(value);

    /// <summary>
    /// Tests if there is the command handler of given identifier.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <returns>true if exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">id was null.</exception>
    public bool ContainsCommandHandler(string id)
        => MainElement.ContainsCommandHandler(id);

    /// <summary>
    /// Tries to get the specific command handler.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <param name="handler">The process handler.</param>
    /// <returns>true if exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">id was null.</exception>
    public bool TryGetCommandHandler(string id, out ILocalWebAppCommandHandler handler)
        => MainElement.TryGetCommandHandler(id, out handler);

    /// <summary>
    /// Tries to get the specific command handler.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <returns>The command handler..</returns>
    /// <exception cref="ArgumentNullException">id was null.</exception>
    public ILocalWebAppCommandHandler TryGetCommandHandler(string id)
        => MainElement.TryGetCommandHandler(id);

    /// <summary>
    /// Registers a command handler. It will override the existed one.
    /// </summary>
    /// <param name="handler">The process handler to add.</param>
    /// <exception cref="ArgumentNullException">id was null.</exception>
    public void RegisterCommandHandler(ILocalWebAppCommandHandler handler)
        => MainElement.RegisterCommandHandler(handler);

    /// <summary>
    /// Removes the command handler.
    /// </summary>
    /// <param name="id">The handler identifier.</param>
    /// <returns>true if remove succeeded; otherwise, false.</returns>
    public bool RemoveCommandHandler(string id)
        => MainElement.RemoveCommandHandler(id);

    /// <summary>
    /// Removes the command handler.
    /// </summary>
    /// <param name="handler">The process handler to remove.</param>
    /// <returns>true if remove succeeded; otherwise, false.</returns>
    public bool RemoveCommandHandler(ILocalWebAppCommandHandler handler)
        => MainElement.RemoveCommandHandler(handler);

    /// <summary>
    /// Maps a folder as a virtual host name.
    /// </summary>
    /// <param name="hostName">The virtual host name.</param>
    /// <param name="folderPath">The folder path to map.</param>
    /// <param name="accessKind">The access kind.</param>
    public void SetVirtualHostNameToFolderMapping(string hostName, string folderPath, CoreWebView2HostResourceAccessKind accessKind)
        => MainElement.SetVirtualHostNameToFolderMapping(hostName, folderPath, accessKind);

    /// <summary>
    /// Clears the mapping of the specific virtual host name.
    /// </summary>
    /// <param name="hostName">The virtual host name.</param>
    public void ClearVirtualHostNameToFolderMapping(string hostName)
        => MainElement.ClearVirtualHostNameToFolderMapping(hostName);

    /// <summary>
    /// Prints to PDF format file.
    /// </summary>
    /// <param name="path">The output path of the PDF format file.</param>
    /// <param name="printSettings">The print settings.</param>
    /// <returns>true if print succeeded; otherwise, false.</returns>
    public Task<bool> PrintToPdfAsync(string path, CoreWebView2PrintSettings printSettings)
        => MainElement.PrintToPdfAsync(path, printSettings);

    /// <summary>
    /// Opens the default download dialog.
    /// </summary>
    public void OpenDefaultDownloadDialog()
        => MainElement.OpenDefaultDownloadDialog();

    /// <summary>
    /// Closes the default download dialog.
    /// </summary>
    public void CloseDefaultDownloadDialog()
        => MainElement.CloseDefaultDownloadDialog();

    internal void SetFullScreen(bool value)
    {
        VisualUtility.SetFullScreenMode(value, this);
        TitleRow.Height = value ? new(0) : new(titleHeight);
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        backdrop?.Dispose();
        MainElement.Close();
        MainElement.WindowController = null;
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

    private void MainElement_ContainsFullScreenElementChanged(object sender, DataEventArgs<bool> e)
        => SetFullScreen(e.Data);
}
